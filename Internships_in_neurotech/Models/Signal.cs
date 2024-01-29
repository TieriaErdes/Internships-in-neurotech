using Avalonia.Controls.Converters;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Internships_in_neurotech.Models
{
    internal class Signal
    {
        private const int numOfTicks = 2500;
        private int fileOffset = 0;

        private byte[] currentPartOfFile = new byte[numOfTicks * sizeof(double)];
        private int currentPosition;

        private string? signalFileName;

        public double[][] DataFromFile;



        public Signal(in SerializedChannel serializedChannel)
        {

            DataFromFile = new double[serializedChannel.bosMeth.Channels.Capacity][];


            //Task[] tasks = new Task[serializedChannel.bosMeth.Channels.Capacity];


            //for (int i = 0; i < tasks.Length; i++)
            //{
            //    DataFromFile[i] = new double[numOfTicks];
            //    tasks[i] = GetDataAsync(serializedChannel.FilePath + serializedChannel.bosMeth.Channels[i].SignalFileName, i);
            //}

            //Task.WaitAll(tasks);

            Methods(serializedChannel);
        }

        private async void Methods(SerializedChannel serializedChannel)
        {
            Task[] tasks = new Task[serializedChannel.bosMeth.Channels.Capacity];


            for (int i = 0;  i < tasks.Length; i++)
            {
                DataFromFile[i] = new double[numOfTicks];
                tasks[i] = GetDataAsync(serializedChannel.FilePath + serializedChannel.bosMeth.Channels[i].SignalFileName, i);

                await tasks[i];
            }

            Task.WaitAll(tasks);
        }

        private async Task GetDataAsync(string signalPath, int num)
        {
            await Task.Run(() =>
            {
                using (FileStream signalFile = new FileStream(signalPath, FileMode.Open))
                {
                    //var stopwatch = Stopwatch.StartNew();
                    signalFile.Read(currentPartOfFile, fileOffset, numOfTicks * sizeof(double));


                    for (int i = 0; i < numOfTicks; i++)
                    {
                        DataFromFile[num][i] = BitConverter.ToDouble(currentPartOfFile, i * 8);

                        //Debug.WriteLine($"{dataFromFile[i]} ---- {i}");
                    }
                    //stopwatch.Stop();
                    //Debug.WriteLine($"Asynchronous file read took {stopwatch.ElapsedMilliseconds} ms");
                    //Debug.WriteLine("ХУЙХУЙХУЙХУЙХУЙХУЙХУЙХУЙХУЙ");
                    Debug.WriteLine($"{num} async thread compleated");
                    Debug.WriteLine($"{Task.CurrentId}");
                };
            });
        }
    }
}
