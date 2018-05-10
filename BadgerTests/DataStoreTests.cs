using Badger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Badger.Tests
{
    [Trait("Category", "Data Store")]
    public class DataStoreTests
    {

        [Fact]
        public void DataStore_Initialize_CreatesEmptyStore()
        {
            DataStore.Initialize();
            Assert.Equal(0, DataStore.Count);
        }

        [Fact]
        public void DataStore_AddString_StoresString()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "something i need to store");
            Assert.Equal(1, DataStore.Count);
            Assert.Equal("something i need to store", DataStore.Get("myKey"));
        }
        [Fact]
        public void DataStore_Retrieve_RetrievesString()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "my string to store");
            Assert.Equal("my string to store", DataStore.Get("myKey"));
        }

        [Fact]
        public void DataStore_AddObject_StoresObject()
        {
            DataStore.Initialize();
            var myObject = new List<string>() { "Buzz", "Lightyear" };
            DataStore.Add("myKey", myObject);
            Assert.Equal(1, DataStore.Count);
        }

        [Fact]
        public void DataStore_Retrieve_RetrievesObject()
        {
            DataStore.Initialize();
            var myObject = new List<string>() { "Buzz", "Lightyear" };
            DataStore.Add("myKey", myObject);
            Assert.Equal("Lightyear", DataStore.Get<List<string>>("myKey")[1]);
        }

        [Fact]
        public void DataStore_Remove_RemovesObject()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "something i need to store");
            DataStore.Remove("myKey");
            Assert.Equal(0, DataStore.Count);
        }


    }
}
