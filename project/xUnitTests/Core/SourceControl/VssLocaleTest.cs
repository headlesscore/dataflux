using System;
using System.Globalization;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class VssLocaleTest
	{
		[Fact]
		public void FormatDateInCultureInvariantFormat()
		{
			IVssLocale vssLocale = new VssLocale(CultureInfo.InvariantCulture);
			Assert.Equal("02/22/2002;20:00", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 20, 0, 0)));
			Assert.Equal("02/22/2002;12:00", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 12, 0, 0)));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void FormatDateInUSFormat()
		{
			IVssLocale vssLocale = new VssLocale(UsCuture());
			Assert.Equal("2/22/2002;8:00p", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 20, 0, 0)));
			Assert.Equal("2/22/2002;12:00p", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 12, 0, 0)));
		}

		[Fact]
		public void FormatDateInUKFormat()
		{
			IVssLocale vssLocale = new VssLocale(UkCulture());
			Assert.Equal("22/02/2002;20:00", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 20, 0, 0, 34)));
			Assert.Equal("22/02/2002;12:00", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 12, 0, 0)));
		}

		[Fact]
		public void FormatDateInUKFormatWithAMPMIndicator()
		{
			IVssLocale vssLocale = new VssLocale(UkCultureWithAmPmTimeFormat());
			Assert.Equal("22/02/2002;8:00p", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 20, 0, 0, 34)));
			Assert.Equal("22/02/2002;12:00p", vssLocale.FormatCommandDate(new DateTime(2002, 2, 22, 12, 0, 0)));
		}

		[Fact]
		public void ParseDateAndTime()
		{
			IVssLocale vssLocale = new VssLocale(CultureInfo.InvariantCulture);
			Assert.Equal(new DateTime(2002, 2, 22, 12, 0, 0), vssLocale.ParseDateTime("02/22/2002", "12:00"));
		}

		[Fact]
		public void FormatDateForEnglishServerWithFrenchLocalCulture()
		{
			IVssLocale locale = new VssLocale(FrCulture());
			locale.ServerCulture = "en-US";
			Assert.Equal("added", locale.AddedKeyword);
			Assert.Equal("22/02/2002;12:00", locale.FormatCommandDate(new DateTime(2002, 2, 22, 12, 0, 0)));
		}

		private CultureInfo UsCuture()
		{
			return new CultureInfo("en-US", false);
		}

		private CultureInfo UkCulture()
		{
			return new CultureInfo("en-GB", false);
		}

		private CultureInfo UkCultureWithAmPmTimeFormat()
		{
			CultureInfo cultureInfo = UkCulture();
			cultureInfo.DateTimeFormat.LongTimePattern = cultureInfo.DateTimeFormat.LongTimePattern + " tt";
			return cultureInfo;
		}

		
		private CultureInfo FrCulture()
	{
		return new CultureInfo("fr-FR", false);
		} 
	}
}
