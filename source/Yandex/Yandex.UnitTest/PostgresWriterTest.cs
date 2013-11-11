using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yandex.PostgresWriter;

namespace Yandex.UnitTest
{
    [TestClass]
    public class PostgresWriterTest
    {
        [TestMethod]
        public void QueryExecutionTest()
        {
            var connector = new DbConnector();
            var result = connector.ExecuteQuery("SELECT column1 FROM testschema.\"testTable\";");
            var table = result.Tables[0];

            foreach (var dbRow in table.Rows.Cast<DataRow>())
            {
                Debug.WriteLine(dbRow[0]);
            }
        }
    }
}
