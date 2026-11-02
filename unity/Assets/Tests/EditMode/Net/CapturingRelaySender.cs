using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Net;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>A relay sender that keeps every frame the TV app would send.</summary>
    public sealed class CapturingRelaySender : IRelaySender
    {
        public List<string> Sent { get; } = new List<string>();

        public void Send(string json)
        {
            Sent.Add(json);
        }

        public void Clear()
        {
            Sent.Clear();
        }

        /// <summary>Every frame parsed as JSON, in order.</summary>
        public List<JObject> Frames => Sent.Select(JObject.Parse).ToList();

        /// <summary>The inner messages sent to one conn, in order.</summary>
        public List<JObject> MessagesTo(string conn)
        {
            return Frames.Where(f => (string)f["to"] == conn).Select(f => (JObject)f["msg"]).ToList();
        }

        /// <summary>The inner messages of one type sent to one conn, in order.</summary>
        public List<JObject> MessagesTo(string conn, string type)
        {
            return MessagesTo(conn).Where(m => (string)m["type"] == type).ToList();
        }
    }
}
