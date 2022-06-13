using System;
using System.IO;

namespace Carrot.Device {

    /// <summary>
    /// \class Tools
    /// Gathers static helper functions that can be used for different purposes (e.g., data storage)
    /// </summary>
    internal static class Tools {

        /// <summary>
        /// Saves a string to a file on the disk
        /// </summary>
        /// <param name="filename">target file name and path (string)</param>
        /// <param name="data">text to save (string)</param>
        /// <param name="append">a flag indicating if the file should appended (Boolean)</param>
        public static void SaveData(string filename, string data, bool append = true) {
            Stream stream = null;

            try {
                if (append == true)
                    stream = new FileStream(filename, FileMode.Append);
                else
                    stream = new FileStream(filename, FileMode.Create);

                using (StreamWriter writer = new StreamWriter(stream)) {
                    stream = null;
                    writer.Write(data);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("An error has occured!\nMessage: " + ex.Message + "\nStack Trace: " + ex.StackTrace + "\nSource: " + ex.Source);
            } finally {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}