using System;
using System.Linq;
using System.Threading;
using System.IO.Pipes;
using UnityEngine;
using ProtoBuf;
using Protocol.Rpc;
using Protocol.Rpc.Messages;

namespace GemSDK.Unity
{
    public class WindowsGem : IGem
    {
        private Quaternion last;
        private readonly object lastMutex = new object();

        private Quaternion Last
        {
            get
            {
                Quaternion result;

                lock(lastMutex)
                {
                    result = last;
                }

                return result;
            }

            set
            {
                lock(lastMutex)
                {
                    last = value;
                }
            }
        }

        private Vector3 lastAcceleration;
        private readonly object lastAccelerationMutex = new object();

        private Vector3 LastAcceleration
        {
            get
            {
                Vector3 result;

                lock (lastAccelerationMutex)
                {
                    result = lastAcceleration;
                }

                return result;
            }

            set
            {
                lock (lastAccelerationMutex)
                {
                    lastAcceleration = value;
                }
            }
        }

        private Quaternion reference;
        private NamedPipeClientStream pipe;
        private Thread listeningThread;
        private volatile bool running;

        public WindowsGem()
        {
            last = Quaternion.identity;
            reference = Quaternion.identity;
            lastAcceleration = Vector3.zero;

            pipe = new NamedPipeClientStream(".", "GemPipe", PipeDirection.InOut, PipeOptions.Asynchronous);

            listeningThread = new Thread(new ThreadStart(ListeningThread));
            listeningThread.Name = "GemPipe Reader";
            listeningThread.IsBackground = true;
        }

        //public GemState state { get { return pipe.IsConnected ? GemState.Connected : GemState.Disconnected; } }
        public GemState state { get; private set; }


        public Quaternion rotation { get { return reference * Last; } }

        public Vector3 acceleration { get { return LastAcceleration; } }

        public void Calibrate()
        {
            reference = Quaternion.Inverse(Last);   
        }

        public void Reconnect()
        {

        }

        public void Start()
        {
			running = true;
            listeningThread.Start();
        }

        public void Stop(bool block)
        {
            running = false;

            if (block)
                listeningThread.Join();
        }

        private void ListeningThread()
        {
            // TODO(tomer) use cancelation token

            pipe.Connect();
            pipe.ReadMode = PipeTransmissionMode.Byte;
            
            while (running)
            {
                try
                {
                    int _header = pipe.ReadByte();
                    int _length = pipe.ReadByte();

                    if(_header == -1 || _length == -1)
                    {
                        break; // End of stream
                    }
                    
                    MessageType header = (MessageType)_header;
                    byte length = (byte)_length;
                    byte[] payload = new byte[length];
                    int read = 0;

                    do
                    {
                        read = pipe.Read(payload, read, length - read);
                    } while (read < length);

                    HandleMessage(header, payload);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    throw e;
                }
            }
        }

        private void HandleMessage(MessageType messageType, byte[] payload)
        {
            switch (messageType)
            {
                case MessageType.AdapterStateChanged:
                case MessageType.Error:
                    // TODO(tomer) handle it
                case MessageType.ConnectionStatusChanged:
                    {
                        ConnectionStatusChanged obj = ProtobufSerializer.Deserialize<ConnectionStatusChanged>(payload);
                        state = obj.State == ConnectionStateType.Connected ? GemState.Connected : GemState.Disconnected;
                    }
                    break;
                case MessageType.FusionDataReceived:
                    {
                        FusionDataReceived obj = ProtobufSerializer.Deserialize<FusionDataReceived>(payload);
                        Last = new Quaternion(obj.Quaternion[0], obj.Quaternion[1], obj.Quaternion[2], obj.Quaternion[3]);
                    }
                    break;
                case MessageType.RawDataReceived:
                    {
                        RawDataReceived obj = ProtobufSerializer.Deserialize<RawDataReceived>(payload);
                        LastAcceleration = new Vector3(obj.Accelerometer[0], obj.Accelerometer[1], obj.Accelerometer[2]);
                    }
                    break;
                case MessageType.CombinedDataReceived:
                    {
                        CombinedDataReceived obj = ProtobufSerializer.Deserialize<CombinedDataReceived>(payload);
                        Last = new Quaternion(obj.Quaternion[0], obj.Quaternion[1], obj.Quaternion[2], obj.Quaternion[3]);
                        LastAcceleration = new Vector3(obj.Acceleration[0], obj.Acceleration[1], obj.Acceleration[2]);
                    }
                    break;
                default:
                    throw new Exception("Unknown message received");
            }
        }
    }
}