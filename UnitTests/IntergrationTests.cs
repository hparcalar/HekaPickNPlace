using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

namespace UnitTests
{
    [TestClass]
    public class IntergrationTests
    {
        [TestMethod]
        public void ParseJotunServiceSoapResponse()
        {
            //XmlReaderSettings stg = new XmlReaderSettings();
            //stg.ConformanceLevel = ConformanceLevel.Auto;
            //XmlReader xRead = XmlReader.Create("", stg);

            DataSet ds = new DataSet();
            ds.ReadXml("C:\\Users\\Hakan\\Desktop\\test.xml", XmlReadMode.Fragment);
            if (ds.Tables.Count > 0)
            {
                List<string> itemCodes = new List<string>();

                bool breakUpperLoop = false;

                foreach (DataTable table in ds.Tables)
                {
                    if (breakUpperLoop)
                        break;

                    foreach (DataRow row in table.Rows)
                    {
                        string imCode = (string)row[4];

                        if (itemCodes.Contains(imCode))
                        {
                            breakUpperLoop = true;
                            break;
                        }

                        itemCodes.Add(imCode);

                        // set recipe header information
                        string recipeCode = (string)row[2];
                        string recipeName = (string)row[3];
                        string itemName = (string)row[5];
                        int nof_pac = Convert.ToInt32(row[6]);
                    }
                }

                Assert.IsTrue(true);
            }
        }
    }
}
