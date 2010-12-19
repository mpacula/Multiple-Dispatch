using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MultipleDispatch;

namespace MultipleDispatchTests
{
    [TestFixture]
    class MultimethodTests
    {
        private TestTarget target = new TestTarget();
        private PrettyPrinter pp = new PrettyPrinter();
        object complicatedObject = new object[] { 1, 2f, 3, true, "test", new object[] { "hello", 1234 }, 6.0, "test2" };

        /// <summary>
        /// A sanity test - uses Multimethod to call non-overloaded methods
        /// </summary>
        [Test]
        public void CallNonoverloaded()
        {
            // without arguments
            Assert.AreEqual("test", "test".Call<string>("ToString"));
            Assert.AreEqual("1234",1234.Call<string>("ToString"));

            // with arguments
            Assert.AreEqual(64, target.Call<double>("Square", 8.0));
            Assert.AreEqual("hello, world!", target.Call<string>("Append", "hello, ", "world!"));

            // now test calls that should fail

            // without arguments
            try
            {
                "test".Call<string>("ToString", 1); // spurious argument
                Assert.Fail("Expected multimethod to fail due to a spurious argument");
            }
            catch (NoMatchingMethodException)
            {
            }

            try
            {
                1234.Call<int>("ToString"); // wrong return type
                Assert.Fail("Expected multimethod to fail due to a wrong return type");
            }
            catch (NoMatchingMethodException)
            {
            }

            try
            {
                target.Call<double>("Square", "8"); // wrong argument type
                Assert.Fail("Expected multimethod to fail due to a wrong argument type");
            }
            catch (NoMatchingMethodException)
            {
            }
        }

        [Test]
        public void CallOverloaded()
        {           
            Assert.AreEqual("[INT(1), FLOAT(2), INT(3), ???, STRING(test), [STRING(hello), INT(1234)], DOUBLE(6), STRING(test2)] // test object",
                pp.Call<string>("Print", complicatedObject, "test object")); 
        }

        [Test]
        public void CallVoid()
        {
            target.Call("SaySomething");
            Assert.AreEqual("hi", target.Message);

            target.Call("SaySomething", "hello");
            Assert.AreEqual("hello", target.Message);

            target.Call("SaySomething", 1234);
            Assert.AreEqual("1234", target.Message);
        }

        [Test]
        public void CallWithNull()
        {
            target.Call("SaySomething", new object[] { null });
            Assert.AreEqual(null, target.Message);

            Assert.AreEqual("hi", target.Call<string>("Append", "hi", null));
        }

        /// <summary>
        /// Not a test per se, but a microbenchmark that can be used to test for major
        /// performance improvements/degradations.
        /// </summary>
        [Test]
        public void Benchmark()
        {
            const int RUNS = 10000;
            var start = DateTime.Now;
            for (int i = 0; i < RUNS; i++)
            {
                pp.Call<string>("Print", complicatedObject);
            }
            double elapsed = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine("Microbenchmark operations per second: {0}", (double)RUNS / elapsed);
        }
    }

    /// <summary>
    /// Contains some of the methods we'll be testing the Multimethod on.
    /// The other methods are in PrettyPrinter.
    /// </summary>
    class TestTarget
    {
        public String Message { get; set; }

        public void SaySomething()
        {
            Message = "hi";
        }

        public void SaySomething(string msg)
        {
            Message = msg;
        }

        public void SaySomething(int num)
        {
            Message = "" + num;
        }

        public double Square(double x)
        {
            return x * x;
        }

        public string Append(string a, string b)
        {
            return a + b;
        }
    }
}
