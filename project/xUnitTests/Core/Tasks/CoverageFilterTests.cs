namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core.Tasks;

    
    public class CoverageFilterTests
    {
        [Fact]
        public void ToParamStringGeneratesDefault()
        {
            var threshold = new CoverageFilter();
            var expected = string.Empty;
            var actual = threshold.ToParamString();
            Assert.Equal(expected, actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ToParamStringGeneratesWithClass()
        {
            var threshold = new CoverageFilter
            {
                Data = "SomeData",
                ItemType = CoverageFilter.NCoverItemType.Class
            };
            var expected = "SomeData:Class";
            var actual = threshold.ToParamString();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToParamStringGeneratesWithClassAndRegex()
        {
            var threshold = new CoverageFilter
            {
                Data = "SomeData",
                ItemType = CoverageFilter.NCoverItemType.Document,
                IsRegex = true
            };
            var expected = "SomeData:Document:true:false";
            var actual = threshold.ToParamString();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToParamStringGeneratesWithClassAndInclude()
        {
            var threshold = new CoverageFilter
            {
                Data = "SomeData",
                ItemType = CoverageFilter.NCoverItemType.Document,
                IsInclude = true
            };
            var expected = "SomeData:Document:false:true";
            var actual = threshold.ToParamString();
            Assert.Equal(expected, actual);
        }
    }
}
