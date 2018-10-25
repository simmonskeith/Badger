using Badger.Core;
using FluentAssertions;
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
            DataStore.Count.Should().Be(0);
        }

        [Fact]
        public void DataStore_AddString_StoresString()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "something i need to store");
            DataStore.Count.Should().Be(1);
            DataStore.Get("myKey").Should().Be("something i need to store");
        }
        [Fact]
        public void DataStore_Retrieve_RetrievesString()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "my string to store");
            DataStore.Get("myKey").Should().Be("my string to store");
        }

        [Fact]
        public void DataStore_AddObject_StoresObject()
        {
            DataStore.Initialize();
            var myObject = new List<string>() { "Buzz", "Lightyear" };
            DataStore.Add("myKey", myObject);
            DataStore.Count.Should().Be(1);
        }

        [Fact]
        public void DataStore_Retrieve_RetrievesObject()
        {
            DataStore.Initialize();
            var myObject = new List<string>() { "Buzz", "Lightyear" };
            DataStore.Add("myKey", myObject);
            DataStore.Get<List<string>>("myKey")[1].Should().Be("Lightyear");
        }

        [Fact]
        public void DataStore_Remove_RemovesObject()
        {
            DataStore.Initialize();
            DataStore.Add("myKey", "something i need to store");
            DataStore.Remove("myKey");
            DataStore.Count.Should().Be(0);
        }


    }
}
