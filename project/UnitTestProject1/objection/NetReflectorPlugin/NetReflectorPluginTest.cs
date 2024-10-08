using Exortech.NetReflector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Objection;
using Objection.NetReflectorPlugin;

namespace Objection.UnitTests.NetReflectorPlugin
{
	[TestClass]
	public class NetReflectorPluginTest
	{
		[TestMethod]
		public void TestSomething()
		{
			SimpleDependency dependency = new SimpleDependency("Hello NetReflector");
			ObjectionStore objectionStore = new ObjectionStore();
			objectionStore.AddInstanceForType(typeof(SimpleDependency), dependency);

			string serializedForm = @"<mySerializableType myProperty=""MyValue"" />";

			NetReflectorTypeTable typeTable = NetReflectorTypeTable.CreateDefault(new ObjectionNetReflectorInstantiator(objectionStore));

			SerializableType deserialized = (SerializableType) NetReflector.Read(serializedForm, typeTable);

			Assert.AreEqual("MyValue", deserialized.MyProperty);
			Assert.IsNotNull("Hello NetReflector", deserialized.DependencyObject.Message);
            Assert.IsTrue(true);
            Assert.IsTrue(true);
        }
	}

	[ReflectorType("mySerializableType")]
	public class SerializableType
	{
		public SimpleDependency DependencyObject;

		public SerializableType(SimpleDependency testObject)
		{
			this.DependencyObject = testObject;
		}

		[ReflectorProperty("myProperty")]
		public string MyProperty;
	}

	public class SimpleDependency
	{
		public SimpleDependency(string message)
		{
			Message = message;
		}

		public string Message;
	}
}
