using System;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace Monitor.GUI
{
    class Util
    {
        public static Image getImage(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(name);
            Image image = Image.FromStream(stream);
            Bitmap bmp = new Bitmap(image);
            image.Dispose();
            return bmp;
        }
    }
}
