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
using System.Text.RegularExpressions;
using ExtensionsNamespace;

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
                        Info("New Device wants to connect...");
                        if (Referees.Count == 0)
                        {
                            Info("Can't connect, no available referees. Release any disabled referee!");
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
                            Info($"New device has connected. Sending referee ID {Device.Id}");
                        }
                        else if (Device.Id == packet.Id)
                        {
                            Device.Handle = ws;
                            Info($"Reconnected device, found by ID. Handing it back referee #{Device.Id}");
                        }
                        else
                        {
                            Device.Mac = packet.Mac;
                            Device.Handle = ws;
                            Info($"Reconnected device, found by MAC. Handing it back referee #{Device.Id}");
                        }
                        Device.Enabled = true;
                        var x = FindDevice(ws);
                        var y = OutboundPacket.Instructions("hello", id: x.Id);
                        ws.Send(y);
                        break;


                    case "hello":
                        Info($"Device {Device.Id} confirms referee ID");
                        ws.Send(OutboundPacket.Instructions("idle", idle: true, message: $"Successfully connected as referee #{Device.Id}"));
                        break;


                    case "battery":
                        Info($"Device {Device.Id} battery: {packet.Battery.ToString()}%.");
                        break;


                    case "confirm":
                        DoInMainThread(() => {
                            (ActiveContestantWindow
                                .ScoresGrid
                                .Children[Device.Id - 1] as Grid)
                                .Children
                                .Map((c, i) => i == 1 || i == 2, c => c as TextBox)
                                .Apply(tb => {
                                    tb.BorderBrush = Brushes.Green;
                                    tb.BorderThickness = new Thickness(3);
                                });
                            //new List<string>() { "LScore", "RScore" }.ForEach(s => {
                            //    Node<TextBox>($"{s}{Device.Id}", ActiveContestantWindow).BorderBrush = Brushes.Green;
                            //    Node<TextBox>($"{s}{Device.Id}", ActiveContestantWindow).BorderThickness = new Thickness(3);
                            //});
                        });
                        goto case "score";

                    case "score":
                        
                        Info($"Device {Device.Id} score: {packet.Scores.Accuracy / 10f} | {packet.Scores.Presentation / 10f}.");
                        DoInMainThread(() => {
                            var soloScores = AppContext.Scores.Local.First(s => s.Contestant == ActiveContestantWindow.Contestant)
                                                                    .SoloScores.Where(ss => ss.Index == Device.Id);
                            soloScores.First(ss => ss.Type == "Accuracy").Value = packet.Scores.Accuracy / 10f;
                            soloScores.First(ss => ss.Type == "Presentation").Value = packet.Scores.Presentation / 10f;
                            ActiveContestantController.RaisePropertyChanged("Score");
                        });
                        break;

                    default:
                        Info($"Device {Device.Id} says: {packet.Type}.");
                        break;
                }
            }
            catch (JsonReaderException ex)
            {
                Info(ex.StackTrace);
                Info($"Device {Device.Id} says: {e.Data}.");
            }
            catch (Exception ex)
            {
                Info(ex.StackTrace);
            }
        }


        protected override void OnClose(CloseEventArgs e)
        {
            const int NORMAL_CLOSURE_STATUS = 1000;
            
            Info(e.Reason);
            int id = int.Parse(new Regex(@"\d").Match(e.Reason).Value);
            if (e.Code != NORMAL_CLOSURE_STATUS)
            {
                Devices.RemoveAt(id);
                Referees.Enqueue(id);
                Info($"Referee {id} released");
                base.OnClose(e);
                return;
            }
            FindDevice(id).Enabled = false;
            base.OnClose(e);
        }


        private void DoInMainThread(Action action) => Application.Current.Dispatcher.Invoke(action);
        private void Info(string text)
        {
            DoInMainThread(() => {
                LogWindow.LogBox.Text += $"[{DateTime.Now.ToString(@"HH:mm:ss:ffff")}] INFO {text}{Environment.NewLine}";
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
    }
}
