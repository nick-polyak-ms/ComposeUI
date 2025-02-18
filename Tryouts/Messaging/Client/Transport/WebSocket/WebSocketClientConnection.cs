﻿// Morgan Stanley makes this available to you under the Apache License,
// Version 2.0 (the "License"). You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0.
// 
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership. Unless required by applicable law or agreed
// to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
// or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MorganStanley.ComposeUI.Tryouts.Messaging.Client.Transport.Abstractions;
using MorganStanley.ComposeUI.Tryouts.Messaging.Core.Messages;
using MorganStanley.ComposeUI.Tryouts.Messaging.Core.Serialization;

namespace MorganStanley.ComposeUI.Tryouts.Messaging.Client.Transport.WebSocket;

internal class WebSocketClientConnection : IClientConnection
{
    public WebSocketClientConnection(
        IOptions<MessageRouterWebSocketOptions> options,
        ILogger<WebSocketClientConnection>? logger = null)
    {
        _options = options;
        _logger = logger ?? NullLogger<WebSocketClientConnection>.Instance;
    }

    public async ValueTask DisposeAsync()
    {
        _stopTokenSource.Cancel();

        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                statusDescription: null,
                CancellationToken.None);
        }
    }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(_options.Value.Uri, cancellationToken);
        StartReceivingMessages();
        StartSendingMessages();
    }

    public ValueTask SendAsync(Message message, CancellationToken cancellationToken = default)
    {
        return _outputChannel.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<Message> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        return _inputChannel.Reader.ReadAllAsync(cancellationToken);
    }

    private readonly Channel<Message> _inputChannel = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true
        });

    private readonly ILogger<WebSocketClientConnection> _logger;

    private readonly IOptions<MessageRouterWebSocketOptions> _options;

    private readonly Channel<Message> _outputChannel = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true
        });

    private readonly CancellationTokenSource _stopTokenSource = new();

    private ClientWebSocket _webSocket = new();

    private async void StartReceivingMessages()
    {
        var pipe = new Pipe();

        try
        {
            while (!_webSocket.CloseStatus.HasValue && !_stopTokenSource.IsCancellationRequested)
            {
                var buffer = pipe.Writer.GetMemory(1024 * 4);
                try
                {
                    // The cancellation token is None intentionally, see https://github.com/dotnet/runtime/issues/31566
                    var receiveResult = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        break;
                    }

                    pipe.Writer.Advance(receiveResult.Count);

                    if (receiveResult.EndOfMessage)
                    {
                        await pipe.Writer.FlushAsync(CancellationToken.None);
                        var readResult = await pipe.Reader.ReadAsync(CancellationToken.None);
                        var readBuffer = readResult.Buffer;

                        while (!readBuffer.IsEmpty && TryReadMessage(ref readBuffer, out var message))
                            await _inputChannel.Writer.WriteAsync(message, _stopTokenSource.Token);

                        pipe.Reader.AdvanceTo(readBuffer.Start, readBuffer.End);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _inputChannel.Writer.TryComplete(e);
        }
        finally
        {
            _inputChannel.Writer.TryComplete();
            _outputChannel.Writer.TryComplete();
        }
    }

    private async void StartSendingMessages()
    {
        try
        {
            await foreach (var message in _outputChannel.Reader.ReadAllAsync(_stopTokenSource.Token))
            {
                // TODO: use pooled buffer
                var messageBytes = JsonMessageSerializer.SerializeMessage(message);

                await _webSocket.SendAsync(
                    messageBytes,
                    WebSocketMessageType.Text,
                    WebSocketMessageFlags.EndOfMessage,
                    _stopTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal closure on our end
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception while sending the message: {ExceptionMessage}", e.Message);
        }
    }

    private bool TryReadMessage(ref ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out Message? message)
    {
        var innerBuffer = buffer;

        try
        {
            message = JsonMessageSerializer.DeserializeMessage(ref innerBuffer);
            buffer = buffer.Slice(innerBuffer.Start);

            return true;
        }
        catch
        {
            message = null;

            return false;
        }
    }
}
