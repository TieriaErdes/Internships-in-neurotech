using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Internships_in_neurotech.Models
{
    [XmlRoot (ElementName = "BOSMeth", IsNullable = false)]
    public sealed class BOSMeth 
    {
        [XmlElement(ElementName = "TemplateGUID")]
        public Guid TemplateGUID { get; set; }
        //public _Channels? _Channels { get; set; } = new _Channels()
        //{
        //    new Channel (0, "Signal0.bcf", 1 , 1000),
        //    new Channel (2, "Signal2.bcf", 3 , 1000),
        //    new Channel (5, "Signal5.bcf", 3 , 1000),
        //    new Channel (8, "Signal8.bcf", 3 , 1000),
        //};

        [XmlArray(ElementName = "Channels", IsNullable = false)]
        public Channel[] Channels;

        [XmlArrayItem(ElementName = "Channel", Type = typeof(Channel), IsNullable = true)]
        public Channel[] ChannelsItems;

        public BOSMeth() { }
        public BOSMeth(Guid templateGuid, Channel[] channels)
        {
            TemplateGUID = templateGuid;
            Channels = channels;
        }
    }

    public sealed class Channel
    {
        [XmlAttribute(AttributeName = "UnicNumber")]
        public int UnicNumber;
        [XmlAttribute(AttributeName = "SignalFileName")]
        public string? SignalFileName;
        [XmlAttribute(AttributeName = "Type")]
        public int Type;
        [XmlAttribute(AttributeName = "EffectiveFd")]
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
