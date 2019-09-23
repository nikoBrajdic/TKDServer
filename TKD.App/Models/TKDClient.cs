#define MVVM

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TKD.App.Controllers;
using TKD.App.Models;
using TKD.App.Views;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TKD.App
{
    public class TKDClient : WebSocketBehavior
    {
        public MainWindow BaseWindow { get; set; }
        public LogWindow LogWindow { get; set; }
        public ActiveContestant ActiveContestantWindow { get; set; }
        public ActiveContestantController ActiveContestantController { get; set; }
        public ObservableCollection<Device> Devices { get; set; }
        public Queue<int> Referees { get; set; }
        public TkdModel AppContext { get; set; }

        protected override void OnOpen() { base.OnOpen(); }

        protected override void OnMessage(MessageEventArgs e)
        {
            var ws = Context.WebSocket;
            base.OnMessage(e);

            var Device = FindDevice(ws);

            try
            {
                var packet = JsonConvert.DeserializeObject<InboundPacket>(e.Data);
                switch (packet.Type)
                {
                    case "connect_request":
                        Write("New Device wants to connect...");
                        if (Referees.Count == 0)
                        {
                            Write("Can't connect, no available referees. Release any disabled referee!");
                            // stop connecting till I tell you to start!
                            ws.Send(OutboundPacket.Instructions("disconnect", dc: true, message: "no more available referees!"));
                            ws.Close();
                            break;
                        }

                        if ((Device = FindDevice(packet.Mac)) == null)
                        {
                            Device = new Device
                            {
                                Id = Referees.Dequeue(),
                                Handle = ws,
                                Mac = packet.Mac,
                                Enabled = true
                            };
                            Devices.Add(Device);
                            Write($"New device has connected. Sending referee ID {Device.Id}");
                        }
                        else if (Device.Id == packet.Id)
                        {
                            Device.Handle = ws;
                            Write($"Reconnected device, found by ID. Handing it back referee #{Device.Id}");
                        }
                        else
                        {
                            Device.Mac = packet.Mac;
                            Device.Handle = ws;
                            Write($"Reconnected device, found by MAC. Handing it back referee #{Device.Id}");
                        }
                        Device.Enabled = true;
                        var x = FindDevice(ws);
                        var y = OutboundPacket.Instructions("hello", id: x.Id);
                        ws.Send(y);
                        break;


                    case "hello":
                        Write($"Device {Device.Id} confirms referee ID");
                        ws.Send(OutboundPacket.Instructions("idle", idle: true, message: $"Successfully connected as referee #{Device.Id}"));
                        break;


                    case "battery":
                        Write($"Device {Device.Id} battery: {packet.Battery.ToString()}%.");
                        break;


                    case "confirm":
                        DoInMainThread(() => {
                            new List<string>() { "LScore", "RScore" }.ForEach(score => {
                                Node<TextBox>($"{score}{Device.Id}", ActiveContestantWindow).BorderBrush = Brushes.Green;
                                Node<TextBox>($"{score}{Device.Id}", ActiveContestantWindow).BorderThickness = new Thickness(3);
                            });
                        });
                        goto case "score";

                    case "score":
#if MVVM
                        DoInMainThread(() =>
                        {
                            var score = AppContext.Scores.Local.First(s => s.Contestant == ActiveContestantWindow.Contestant);
                            score.SoloScores.First(ss =>
                                ss.Index == Device.Id &&
                                ss.Type == "Accuracy"
                            ).Value = packet.Scores.Accuracy / 10f;
                            score.SoloScores.First(ss =>
                                ss.Index == Device.Id &&
                                ss.Type == "Presentation"
                            ).Value = packet.Scores.Presentation / 10f;
                        });
                        ActiveContestantController.RaisePropertyChanged("Score");
                        break;
#else
                        var lval = packet.Astr();
                        var rval = packet.Pstr();
                        Write($"Device {Device.Id} score: {lval} | {rval}.");
                        DoInMainThread(() => {
                            Node<TextBox>($"LScore{Device.Id}", ActiveContestantWindow).Text = lval;
                            Node<TextBox>($"RScore{Device.Id}", ActiveContestantWindow).Text = rval;
                            typeof(Score).GetProperty($"Accuracy{Device.Id}").SetValue(ActiveContestantWindow.Score, double.Parse(lval));
                            typeof(Score).GetProperty($"Presentation{Device.Id}").SetValue(ActiveContestantWindow.Score, double.Parse(rval));
                        });
                        break;
#endif

                    default:
                        Write($"Device {Device.Id} says: {packet.Type}.");
                        break;
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine(ex.StackTrace);
                DoInMainThread(() =>
                    Write($"Device {Device.Id} says: {e.Data}."));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }


        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            var Device = FindDevice(Context.WebSocket);
            DoInMainThread(() => {
                Write($"Device {Device.Id} disconnected.");
                Device.Enabled = false;
            });
        }


        private void DoInMainThread(Action action) => Application.Current.Dispatcher.Invoke(action);
        private void Write(string text)
        {
            DoInMainThread(() => {
                LogWindow.LogBox.Text += text + Environment.NewLine;
                LogWindow.LogBox.ScrollToEnd();
            });
        }

        private Device FindDevice<T>(T param)
        {
            switch (param)
            {
                case int i:
                    return Devices.FirstOrDefault(d => d.Id == i);
                case string s:
                    return Devices.FirstOrDefault(d => d.Mac == s);
                case WebSocket ws:
                    return Devices.FirstOrDefault(d => d.Handle == ws);
                case bool b:
                    return Devices.FirstOrDefault(d => d.Enabled == b);
                default:
                    return null;
            }
        }
        private T Node<T>(string name, UIElement parent = null) => (T)(object)LogicalTreeHelper.FindLogicalNode(parent, name);

    }
}
