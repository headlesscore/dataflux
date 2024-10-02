using NUnit.Framework;
using NUnit.Framework.Legacy;
using Objection;

namespace Objection.UnitTests.AcceptanceTests
{
	// Note - throughout , 'CD' means 'Constructor Dependency'
	[TestFixture]
	public class TypicalUsageTests
	{
		private ObjectionStore store;
		private object testObject;

		[SetUp]
		public void Setup()
		{
			store = new ObjectionStore();
			testObject = new TestClass();
		}

		[Test]
		public void ShouldReturnInstanceRegisteredByType()
		{
			store.AddInstanceForType(typeof(TestInterface), testObject);
			ClassicAssert.AreSame(testObject, store.GetByType(typeof(TestInterface)));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldReturnInstanceRegisteredById()
		{
			store.AddInstanceForName("myObject", testObject);
			ClassicAssert.AreSame(testObject, store.GetByName("myObject"));
		}

		[Test]
		public void ShouldReturnObjectRegisteredByTypeUsingImplementationTypeOfRegisteredObjectIfRegistrationTypeNotSpecified()
		{
			store.AddInstance(testObject);
			ClassicAssert.AreSame(testObject, store.GetByType(typeof(TestClass)));
		}

		[Test]
		public void ShouldConstructAnObjectThatHasNoCDs()
		{
			object constructed = store.GetByType(typeof(TestClass));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsTrue(constructed is TestClass);
		}

		[Test]
		public void ShouldConstructAnObjectThatHasNoCDsWhenReferencedById()
		{
			store.AddTypeForName("foo", typeof(TestClass));
			object constructed = store.GetByName("foo");

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsTrue(constructed is TestClass);
		}

		[Test]
		public void ShouldConstructAnObjectThatHasClassesForCDsByInstantiatingDependenciesIfTheyAreNotRegistered()
		{
			TestClassWithClassDependencies constructed = (TestClassWithClassDependencies) store.GetByType(typeof(TestClassWithClassDependencies));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
		}

		[Test]
		public void ShouldConstructAnObjectThatHasClassesForCDsByInstantiatingDependenciesIfTheyAreNotRegisteredWhenReferencedById()
		{
			store.AddTypeForName("foo", typeof(TestClassWithClassDependencies));
			TestClassWithClassDependencies constructed = (TestClassWithClassDependencies) store.GetByName("foo");

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
		}


		[Test]
		public void ShouldConstructAnObjectThatHasClassesForCDsByUsingRegisteredInstancesIfTheyAreRegistered()
		{
			TestClass dependency = new TestClass();
			store.AddInstanceForType(typeof(TestClass), dependency);
			TestClassWithClassDependencies constructed = (TestClassWithClassDependencies) store.GetByType(typeof(TestClassWithClassDependencies));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.AreSame(dependency, constructed.Dependency);
		}

		[Test]
		public void ShouldConstructAnObjectThatHasInterfacesForCDsByInstantiatingDependenciesIfTheyAreNotRegistered()
		{
			TestClassWithInterfaceDependencies constructed = (TestClassWithInterfaceDependencies) store.GetByType(typeof(TestClassWithInterfaceDependencies));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
		}

		[Test]
		public void ShouldConstructAnObjectThatHasInterfacesForCDsByUsingRegisteredInstancesIfTheyAreRegistered()
		{
			TestClass dependency = new TestClass();
			store.AddInstanceForType(typeof(TestInterface), dependency);
			TestClassWithInterfaceDependencies constructed = (TestClassWithInterfaceDependencies) store.GetByType(typeof(TestClassWithInterfaceDependencies));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
		}

		[Test]
		public void ShouldUseRuntimeSpecifiedDependencyTypeWhenMultipleImplementationsAvailable()
		{
			store.SetDependencyImplementationForType(typeof(ClassThatDependsOnMultiImplInterface), typeof(InterfaceWithMultipleImplementations), typeof(MultiImplTwo));
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByType(typeof(ClassThatDependsOnMultiImplInterface));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is MultiImplTwo);
		}

		[Test]
		public void ShouldUseRuntimeImplementationTypeWhenMultipleImplementationsAvailable()
		{
			store.SetImplementationType(typeof(InterfaceWithMultipleImplementations), typeof(MultiImplOne));
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByType(typeof(ClassThatDependsOnMultiImplInterface));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is MultiImplOne);
		}

		[Test]
		public void ShouldUseRuntimeSpecifiedDependencyTypeOverImplementationTypeWhenMultipleImplementationsAvailable()
		{
			store.SetDependencyImplementationForType(typeof(ClassThatDependsOnMultiImplInterface), typeof(InterfaceWithMultipleImplementations), typeof(MultiImplTwo));
			store.SetImplementationType(typeof(InterfaceWithMultipleImplementations), typeof(MultiImplOne));
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByType(typeof(ClassThatDependsOnMultiImplInterface));

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is MultiImplTwo);
		}

		[Test]
		public void ShouldBeAbleToAddDecoratorsForAGivenIdentifiedImplementation()
		{
			store.AddTypeForName("foo", typeof(MultiImplOne)).Decorate(typeof(DecoratingMultiImpl)).Decorate(typeof(ClassThatDependsOnMultiImplInterface));
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByName("foo");

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is DecoratingMultiImpl);
			ClassicAssert.IsNotNull(((DecoratingMultiImpl) constructed.Dependency).Dependency);
			ClassicAssert.IsTrue(((DecoratingMultiImpl) constructed.Dependency).Dependency is MultiImplOne);
		}

		[Test]
		public void ShouldBeAbleToAddDecoratorsForAGivenIdentifiedInstance()
		{
			MultiImplOne instance = new MultiImplOne();
			store.AddInstanceForName("foo", instance).Decorate(typeof(DecoratingMultiImpl)).Decorate(typeof(ClassThatDependsOnMultiImplInterface));
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByName("foo");

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is DecoratingMultiImpl);
			ClassicAssert.IsNotNull(((DecoratingMultiImpl) constructed.Dependency).Dependency);
			ClassicAssert.AreSame(instance, ((DecoratingMultiImpl) constructed.Dependency).Dependency);
		}

		[Test]
		public void ShouldBeAbleToMarkNMockClassesAsIgnoredForImplementationResolution()
		{
			NMockAwareImplementationResolver resolver = new NMockAwareImplementationResolver();
			resolver.IgnoreNMockImplementations = true;
			store = new ObjectionStore(resolver, new MaxLengthConstructorSelectionStrategy());
			ClassicAssert.IsTrue(store.GetByType(typeof(InterfaceForIgnoring)) is InterfaceForIgnoringImpl);

		}

		[Test]
		public void ShouldBeAbleToSetupDependencyImplementationsForIdentifiers()
		{
			store.AddTypeForName("foo", typeof(ClassThatDependsOnMultiImplInterface));
			store.SetDependencyImplementationForName("foo", typeof(InterfaceWithMultipleImplementations), typeof(MultiImplTwo));
			
			ClassThatDependsOnMultiImplInterface constructed = (ClassThatDependsOnMultiImplInterface) store.GetByName("foo");

			ClassicAssert.IsNotNull(constructed);
			ClassicAssert.IsNotNull(constructed.Dependency);
			ClassicAssert.IsTrue(constructed.Dependency is MultiImplTwo);
		}
	}
}
