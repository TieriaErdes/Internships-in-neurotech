using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Internships_in_neurotech.Models
{
    public class SerializedChannel
    {

        public BOSMeth? bosMeth = new BOSMeth(Guid.Empty);

        private XmlSerializer formatter = new XmlSerializer(typeof(BOSMeth));

        public string FilePath = @"C:\Avalonia\Internships-in-neurotech\Internships_in_neurotech\Models\";
        private string fileName = "C:\\Avalonia\\Internships-in-neurotech\\Internships_in_neurotech\\Models\\MethDescription.xml";

        private void DeserializeData()
            {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                bosMeth = formatter.Deserialize(fileStream) as BOSMeth;
                //var obj = formatter.Deserialize(fileStream);

                if (bosMeth != null)
                    foreach (var channel in bosMeth.Channels)
                        Debug.WriteLine($"signal {channel.SignalFileName}: it is {channel.Type} type");
            }
        }

        public void SerializeData(ref string _fileName)
        {
            if (bosMeth == null)
            {
                throw new Exception("It is impossible to serialize channels into an xml file. bosMeth equals to null");
            }

            if (_fileName != fileName)
                fileName = _fileName;

            //using (FileStream fileStream = new FileStream("C:\\Users\\User\\Desktop\\Програмирование\\Learning Avalonia UI\\Internships-in-neurotech\\Internships_in_neurotech\\Models\\TestMethDescription.xml", FileMode.OpenOrCreate))
            using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fileStream, bosMeth);
            }
        }

        /// <summary>
        /// Constructor for deserialize data from xml file
        /// </summary>
        public SerializedChannel() 
        {
            DeserializeData();
           //SerializeData(ref fileName);
        }
        /// <summary>
        /// Constructor for deserialize data to internal field of class
        /// and serialize this data to the new file or update old file
        /// </summary>
        /// <param name="_fileName"></param>
        public SerializedChannel(string _fileName)
        {
            DeserializeData();
            SerializeData(ref _fileName);
        }
    }
}
