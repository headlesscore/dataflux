namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    public class ItemStatusTests
    {
        #region Tests
        [Test]
        public void ConstructorSetsName()
        {
            var item = new ItemStatus("theName");
            ClassicAssert.AreEqual("theName", item.Name);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void AllPropertiesCanBeSetAndRetrieved()
        {
            var item = new ItemStatus();
            var theName = "myItemName";
            var theDesc = "A description";
            var theError = "any error here";
            var theStatus = ItemBuildStatus.CompletedFailed;
            var startTime = new DateTime(2010, 1, 1, 12, 12, 12);
            var completedTime = new DateTime(2010, 1, 1, 13, 12, 12);
            var estimatedTime = new DateTime(2010, 1, 1, 14, 12, 12);
            var theParent = new ItemStatus();

            item.Name = theName;
            item.Description = theDesc;
            item.Error = theError;
            item.Status = theStatus;
            item.TimeStarted = startTime;
            item.TimeCompleted = completedTime;
            item.TimeOfEstimatedCompletion = estimatedTime;
            item.Parent = theParent;

            ClassicAssert.AreEqual(theName, item.Name);
            ClassicAssert.AreEqual(theDesc, item.Description);
            ClassicAssert.AreEqual(theError, item.Error);
            ClassicAssert.AreEqual(theStatus, item.Status);
            ClassicAssert.AreEqual(startTime, item.TimeStarted);
            ClassicAssert.AreEqual(completedTime, item.TimeCompleted);
            ClassicAssert.AreEqual(estimatedTime, item.TimeOfEstimatedCompletion);
            ClassicAssert.AreEqual(theParent, item.Parent);
            ClassicAssert.AreNotEqual(item.Identifier, item.Parent.Identifier);
        }

        [Test]
        public void CloneGeneratesANewIdenticialInstance()
        {
            var item = new ItemStatus();
            var theName = "myItemName";
            var theDesc = "A description";
            var theError = "any error here";
            var theStatus = ItemBuildStatus.CompletedFailed;
            var startTime = new DateTime(2010, 1, 1, 12, 12, 12);
            var completedTime = new DateTime(2010, 1, 1, 13, 12, 12);
            var estimatedTime = new DateTime(2010, 1, 1, 14, 12, 12);
            var aChild = new ItemStatus("aChild");

            item.Name = theName;
            item.Description = theDesc;
            item.Error = theError;
            item.Status = theStatus;
            item.TimeStarted = startTime;
            item.TimeCompleted = completedTime;
            item.TimeOfEstimatedCompletion = estimatedTime;
            item.ChildItems.Add(aChild);
            var clone = item.Clone();

            ClassicAssert.AreEqual(theName, clone.Name);
            ClassicAssert.AreEqual(theDesc, clone.Description);
            ClassicAssert.AreEqual(theError, clone.Error);
            ClassicAssert.AreEqual(theStatus, clone.Status);
            ClassicAssert.AreEqual(startTime, clone.TimeStarted);
            ClassicAssert.AreEqual(completedTime, clone.TimeCompleted);
            ClassicAssert.AreEqual(estimatedTime, clone.TimeOfEstimatedCompletion);
            ClassicAssert.AreEqual(item.Identifier, clone.Identifier);
            ClassicAssert.AreEqual(1, clone.ChildItems.Count);
            ClassicAssert.AreEqual("aChild", clone.ChildItems[0].Name);
            ClassicAssert.AreEqual(aChild.Identifier, clone.ChildItems[0].Identifier);
        }

        [Test]
        public void GetHashCodeReturnsHashOfIdentifier()
        {
            var item = new ItemStatus();
            var hash = item.GetHashCode();
            ClassicAssert.AreEqual(item.Identifier.GetHashCode(), hash);
        }

        [Test]
        public void EqualsReturnsTrueForSameIdentifier()
        {
            var item1 = new ItemStatus();
            var item2 = item1.Clone();
            ClassicAssert.IsTrue(item1.Equals(item2));
        }

        [Test]
        public void EqualsReturnsFalseForDifferentIdentifier()
        {
            var item1 = new ItemStatus();
            var item2 = new ItemStatus();
            ClassicAssert.IsFalse(item1.Equals(item2));
        }

        [Test]
        public void EqualsReturnsFalseForNonItemStatus()
        {
            var item1 = new ItemStatus();
            var item2 = "This is a test";
            ClassicAssert.IsFalse(item1.Equals(item2));
        }

        [Test]
        public void ToStringGeneratesXml()
        {
            var item = new ItemStatus();
            var theName = "myItemName";
            var theDesc = "A description";
            var theError = "any error here";
            var theStatus = ItemBuildStatus.CompletedFailed;
            var startTime = new DateTime(2010, 1, 1, 12, 12, 12);
            var completedTime = new DateTime(2010, 1, 1, 13, 12, 12);
            var estimatedTime = new DateTime(2010, 1, 1, 14, 12, 12);
            var aChild = new ItemStatus("aChild");

            item.Name = theName;
            item.Description = theDesc;
            item.Error = theError;
            item.Status = theStatus;
            item.TimeStarted = startTime;
            item.TimeCompleted = completedTime;
            item.TimeOfEstimatedCompletion = estimatedTime;
            item.ChildItems.Add(aChild);
            var xml = item.ToString();
            var expected = "<itemStatus xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" name=\"myItemName\" status=\"CompletedFailed\">" +
                "<description>A description</description>" + 
                "<error>any error here</error>" + 
                "<timeStarted>2010-01-01T12:12:12</timeStarted>" + 
                "<timeCompleted>2010-01-01T13:12:12</timeCompleted>" + 
                "<timeOfEstimatedCompletion>2010-01-01T14:12:12</timeOfEstimatedCompletion>" + 
                "<childItems>" + 
                    "<childItem name=\"aChild\" status=\"Unknown\">" + 
                        "<timeStarted xsi:nil=\"true\" />" + 
                        "<timeCompleted xsi:nil=\"true\" />" +
                        "<timeOfEstimatedCompletion xsi:nil=\"true\" />" + 
                        "<childItems />" + 
                    "</childItem>" + 
                "</childItems>" + 
                "</itemStatus>";
            //ClassicAssert.AreEqual(expected, xml);

            XDocument.Parse(xml).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            var item = new ItemStatus();
            var theName = "myItemName";
            var theDesc = "A description";
            var theError = "any error here";
            var theStatus = ItemBuildStatus.CompletedFailed;
            var startTime = new DateTime(2010, 1, 1, 12, 12, 12);
            var completedTime = new DateTime(2010, 1, 1, 13, 12, 12);
            var estimatedTime = new DateTime(2010, 1, 1, 14, 12, 12);
            var aChild = new ItemStatus("aChild");

            item.Name = theName;
            item.Description = theDesc;
            item.Error = theError;
            item.Status = theStatus;
            item.TimeStarted = startTime;
            item.TimeCompleted = completedTime;
            item.TimeOfEstimatedCompletion = estimatedTime;
            item.ChildItems.Add(aChild);
            var result = TestHelpers.RunSerialisationTest(item);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsInstanceOf<ItemStatus>(result);
            var actualStatus = result as ItemStatus;
            ClassicAssert.AreEqual(theName, actualStatus.Name);
            ClassicAssert.AreEqual(theDesc, actualStatus.Description);
            ClassicAssert.AreEqual(theError, actualStatus.Error);
            ClassicAssert.AreEqual(theStatus, actualStatus.Status);
            ClassicAssert.AreEqual(startTime, actualStatus.TimeStarted);
            ClassicAssert.AreEqual(completedTime, actualStatus.TimeCompleted);
            ClassicAssert.AreEqual(estimatedTime, actualStatus.TimeOfEstimatedCompletion);
            ClassicAssert.AreEqual(item.Identifier, actualStatus.Identifier);
            ClassicAssert.AreEqual(1, actualStatus.ChildItems.Count);
            ClassicAssert.AreEqual("aChild", actualStatus.ChildItems[0].Name);
            ClassicAssert.AreEqual(aChild.Identifier, actualStatus.ChildItems[0].Identifier);
        }
        #endregion
    }
}
