/*
 * CSharpBcp
 * Copyright 2014 深圳岂凡网络有限公司 (Shenzhen QiFun Network Corp., LTD)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
namespace Qifun.Bcp
{
    public static class Bcp
    {
        public const uint MaxOfflinePack = 200;
        public const uint MaxActiveConnectionsPerSession = 3;
        public const uint MaxConnectionsPerSession = 5;
        public const uint HeartBeatDelayMilliseconds = 3000;
        public const uint ReadingTimeoutMilliseconds = 6000;
        public const uint WritingTimeoutMilliseconds = 1000;
        public const uint BusyTimeoutMilliseconds = 500;
        public const uint ReconnectTimeoutMilliseconds = 500;
        public const uint IdleTimeoutMilliseconds = 10000;
        public const int NumBytesSessionId = 16;
        public const uint MaxDataSize = 100000;

        public struct ConnectionHead
        {
            public readonly byte[] SessionId;
            public readonly bool IsRenew;
            public readonly uint ConnectionId;
            public ConnectionHead(byte[] sessionId, bool isRenew, uint connectionId)
            {
                SessionId = sessionId;
                IsRenew = isRenew;
                ConnectionId = connectionId;
            }
        }

        public interface IPacket { }
        public interface IServerToClient : IPacket { }
        public interface IClientToServer : IPacket { }
        public interface IAcknowledgeRequired : IPacket { }
        public interface IRetransmission : IPacket
        {
            uint ConnectionId { get; }
            uint PackId { get; }
        }

        public struct Data : IClientToServer, IServerToClient, IAcknowledgeRequired
        {
            public const byte HeadByte = 0;

            public readonly IList<ArraySegment<byte>> Buffers;

            public Data(IList<ArraySegment<byte>> buffers)
            {
                Buffers = buffers;
            }
        }

        public struct Acknowledge : IClientToServer, IServerToClient
        {
            public const byte HeadByte = 1;
        }

        public struct RetransmissionData : IClientToServer, IServerToClient, IAcknowledgeRequired, IRetransmission
        {
            public const byte HeadByte = 2;

            public readonly uint ConnectionId;
            public readonly uint PackId;
            public readonly IList<ArraySegment<Byte>> Buffers;

            public RetransmissionData(uint connectionId, uint packId, IList<ArraySegment<Byte>> buffers)
            {
                ConnectionId = connectionId;
                PackId = packId;
                Buffers = buffers;
            }

            uint IRetransmission.ConnectionId { get { return ConnectionId; } }
            uint IRetransmission.PackId { get { return PackId; } }
        }

        public struct Finish : IClientToServer, IServerToClient, IAcknowledgeRequired
        {
            public const byte HeadByte = 3;
        }

        public struct RetransmissionFinish : IClientToServer, IServerToClient, IAcknowledgeRequired, IRetransmission
        {
            public const byte HeadByte = 4;

            public readonly uint ConnectionId;
            public readonly uint PackId;

            public RetransmissionFinish(uint connectionId, uint packId)
            {
                ConnectionId = connectionId;
                PackId = packId;
            }

            uint IRetransmission.ConnectionId { get { return ConnectionId; } }
            uint IRetransmission.PackId { get { return PackId; } }
        }

        public struct ShutDown : IClientToServer, IServerToClient
        {
            public const byte HeadByte = 5;
        }

        public struct HeartBeat : IClientToServer, IServerToClient
        {
            public const byte HeadByte = 6;
        }

        public enum ConnectionState { ConnectionIdle, ConnectionBusy, ConnectionSlow }

        public class ReadState
        {
            public Timer readTimeoutTimer;
            public bool isCancel = false;
            public void Cancel()
            {
                isCancel = true;
                if (!isCancel && readTimeoutTimer != null)
                {
                    readTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    readTimeoutTimer.Dispose();
                    readTimeoutTimer = null;
                }
            }
        }

    }
}

