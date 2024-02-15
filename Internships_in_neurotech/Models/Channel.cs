using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Internships_in_neurotech.Models
{
    public record class BOSMeth 
    {
        [XmlAttribute]
        public Guid TemplateGUID { get; set; }
        //public Channels? Channels { get; set; } = new Channels()
        //{
        //    new Channel (0, "Signal0.bcf", 1 , 1000),
        //    new Channel (2, "Signal2.bcf", 3 , 1000),
        //    new Channel (5, "Signal5.bcf", 3 , 1000),
        //    new Channel (8, "Signal8.bcf", 3 , 1000),
        //};
        public Channels? Channels { get; set; }

        public BOSMeth() { }
        public BOSMeth(Guid templateGuid, Channels? channels)
        {
            TemplateGUID = templateGuid;
            Channels = channels;
        }
    }

    public class Channels: List<Channel>  
    {
        public Channels() { }
    }

    public class Channel
    {
        [XmlAttribute]
        public int UnicNumber;
        [XmlAttribute]
        public string? SignalFileName;
        [XmlAttribute]
        public int Type;
        [XmlAttribute]
        public int EffectiveFd;

        public Channel() { }
        public Channel(int unicNumber, string signalFileName, int type, int effectiveFd)
        {
            UnicNumber = unicNumber;
            SignalFileName = signalFileName;
            Type = type;
            EffectiveFd = effectiveFd;
        }
    }
}
