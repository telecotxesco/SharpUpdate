using System;

namespace SharpUpdate
{
    internal class SharpUpdateXmlFile
    {
        private Uri uri;
        private string fileName;
        private string md5;

        /// <summary>
        /// The location of the update binary
        /// </summary>
        public Uri Uri
        {
            get { return this.uri; }
            set { this.uri = value; }
        }

        /// <summary>
        /// The file name of the binary
        /// for use on local computer
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        /// <summary>
        /// The MD5 of the update's binary
        /// </summary>
        public string MD5
        {
            get { return this.md5; }
            set { this.md5 = value; }
        }
    }
}
