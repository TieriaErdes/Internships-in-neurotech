using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Internships_in_neurotech.Models
{
    public sealed class SerializedChannel
    {
        public BOSMeth? bosMeth;

        private XmlSerializer _formatter = new XmlSerializer(typeof(BOSMeth));

        /// <summary>
        /// полный путь к обрабатываемым файлам
        /// </summary>
        public string? DirectoryPath;

        private const string _fileName = @"MethDescription.xml";

        // Десериализатор 
        private void DeserializeData()
        {
            try
            {
                using (FileStream fileStream = new FileStream(Path.Combine(DirectoryPath!, _fileName), FileMode.Open))
                {
                    bosMeth = _formatter.Deserialize(fileStream) as BOSMeth ?? throw new Exception("Deserializing xml file is failed");

                    if (bosMeth.Channels == null) throw new Exception("Channel list equals to null");

                    foreach (var channel in bosMeth.Channels)
                        Debug.WriteLine($"signal {channel.SignalFileName}: it is {channel.Type} type");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Deserializing failed. Cause: {e.Message}");
            }
        }


        /// <summary>
        /// Constructor for deserialize data from xml file
        /// </summary>
        public SerializedChannel(string path)
        {
            //DirectoryPath = Path.GetDirectoryName(path) + "\\" ?? throw new Exception($"DirectoryPath is equals to null");
            DirectoryPath = Path.Join(path, Path.DirectorySeparatorChar.ToString()); 
            DeserializeData();
        }
    }
}
