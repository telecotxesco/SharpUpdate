using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace SharpUpdate
{
    /// <summary>
    /// Contains update information
    /// </summary>
    internal class SharpUpdateXml
    {
        private Version version;
        private List<SharpUpdateXmlFile> files;
        private string description;
        private string launchFile;
        private string launchArgs;

        /// <summary>
        /// The update version #
        /// </summary>
        internal Version Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// The location of the update binary
        /// </summary>
        internal List<SharpUpdateXmlFile> Files
        {
            get { return this.files; }
        }

        /// <summary>
        /// The update's description
        /// </summary>
        internal string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// The application to start
        /// </summary>
        internal string LaunchFile
        {
            get { return this.launchFile; }
        }

        /// <summary>
        /// The arguments to pass to the updated application on startup
        /// </summary>
        internal string LaunchArgs
        {
            get { return this.launchArgs; }
        }

        /// <summary>
        /// Creates a new SharpUpdateXml object
        /// </summary>
        internal SharpUpdateXml(Version version, List<SharpUpdateXmlFile> files, string description, string launchFile, string launchArgs)
        {
            this.version = version;
            this.files = files;
            this.description = description;
            this.launchFile = launchFile;
            this.launchArgs = launchArgs;
        }

        /// <summary>
        /// Checks if update's version is newer than the old version
        /// </summary>
        /// <param name="version">Application's current version</param>
        /// <returns>If the update's version # is newer</returns>
        internal bool IsNewerThan(Version version)
        {
            return this.version > version;
        }

        /// <summary>
        /// Checks the Uri to make sure file exist
        /// </summary>
        /// <param name="location">The Uri of the update.xml</param>
        /// <returns>If the file exists</returns>
        internal static bool ExistsOnServer(Uri location)
        {
            try
            {
                // Request the update.xml
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(location.AbsoluteUri);
                // Read for response
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                resp.Close();

                return resp.StatusCode == HttpStatusCode.OK;
            }
            catch { return false; }
        }

        /// <summary>
        /// Parses the update.xml into SharpUpdateXml object
        /// </summary>
        /// <param name="location">Uri of update.xml on server</param>
        /// <param name="appID">The application's ID</param>
        /// <returns>The SharpUpdateXml object with the data, or null of any errors</returns>
        internal static SharpUpdateXml Parse(Uri location, string appID)
        {
            Version version = null;
            List<SharpUpdateXmlFile> files = new List<SharpUpdateXmlFile>();
            string description = "", launchArgs = "", launchFile = "";

            try
            {
                // Load the document
				ServicePointManager.ServerCertificateValidationCallback = (s, ce, ch, ssl) => true;
				XmlDocument doc = new XmlDocument();
				doc.Load(location.AbsoluteUri);

                // Gets the appId's node with the update info
                // This allows you to store all program's update nodes in one file
                XmlNode updateNode = doc.DocumentElement.SelectSingleNode("//update[@appID='" + appID + "']");

                // If the node doesn't exist, there is no update
                if (updateNode == null)
                    return null;

                // Parse data
                version = Version.Parse(updateNode["version"].InnerText);
                description = updateNode["description"].InnerText;
                launchFile = updateNode["launchFile"].InnerText;
                launchArgs = updateNode["launchArgs"].InnerText;

                XmlNodeList filesNodeList = doc.DocumentElement.GetElementsByTagName("file");

                foreach (XmlNode fileNode in filesNodeList)
                {
                    files.Add(new SharpUpdateXmlFile() {
                        Uri = new Uri(fileNode["url"].InnerText),
                        FileName = fileNode["fileName"].InnerText,
                        MD5 = fileNode["md5"].InnerText
                    });
                }

				return new SharpUpdateXml(version, files, description, launchFile, launchArgs);
            }
			catch { return null; }
        }
    }
}
