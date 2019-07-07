using Newtonsoft.Json;
using System;

namespace TKD.App
{
    [Serializable]
    class OutboundPacket
    {
        public string Type  { get; set; }
        public int?   Id    { get; set; }
        public bool?  Idle  { get; set; }
        public bool?  Off   { get; set; }
        public bool?  Disconnect { get; set; }
        public bool?  Battery    { get; set; }
        public bool?  Scores     { get; set; }
        public bool?  Vibrate    { get; set; }
        public string Message    { get; set; } = "";
        public Scores SetScores { get; set; } = null;

        /// <summary>
        /// Method <c>Instructions</c> creates a new <c>OutboundPacket</c> object and returns its JSON string representation. Parameters that are not initialised will be excluded.
        /// </summary>
        /// <param name="type">Message header.</param>
        /// <param name="id">Message header.</param>
        /// <param name="off">Switch off the screen.</param>
        /// <param name="dc">Disconnect and retry.</param>
        /// <param name="idle">Disable buttons, go idle.</param>
        /// <param name="vibrate">Vibrate the device.</param>
        /// <param name="scores">Send back current scores.</param>
        /// <param name="battery">Send back battery status.</param>
        /// <param name="message">Extra text to send to the device.</param>
        /// <returns>JSON string representation of the instanced object.</returns>
        public static string Instructions(string type, int? id = null,
                bool? off = null, bool? dc = null, bool? idle = null, 
                bool? vibrate = null, bool? scores = null, 
                bool? battery = null, string message = "", Scores setScores = null) =>
            JsonConvert.SerializeObject(
                new OutboundPacket
                {
                    Type = type,
                    Id = id,
                    Disconnect = dc,
                    Off = off,
                    Idle = idle,
                    Vibrate = vibrate,
                    Scores = scores,
                    Battery = battery,
                    Message = message,
                    SetScores = setScores
                },
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    }
}
