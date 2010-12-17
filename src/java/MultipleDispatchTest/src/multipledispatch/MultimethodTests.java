package multipledispatch;

import junit.framework.Assert;

import org.junit.Test;

/**
 * Tests the Multimethod.
 * 
 * @author "Maciej Pacula (maciej.pacula@gmail.com)"
 *
 */
public class MultimethodTests {
        private TestTarget target = new TestTarget();
        private PrettyPrinter pp = new PrettyPrinter();
        Object complicatedObject = new Object[] { 1, 2f, 3, true, "test", new Object[] { "hello", 1234 }, 6.0, "test2" };

        /**
         * A sanity test - uses Multimethod to call non-overloaded methods
         */
        @Test
        public void callNonoverloaded()
        {
            // without arguments
            Assert.assertEquals("test", Multimethod.call(String.class, "test", "toString"));
            Assert.assertEquals("1234", Multimethod.call(String.class, 1234, "toString"));

            // with arguments
            Assert.assertEquals(64.0, Multimethod.call(double.class, target, "square", 8.0));
            Assert.assertEquals("hello, world!", Multimethod.call(String.class, target, "append", "hello, ", "world!"));

            // now test calls that should fail

            // without arguments
            try
            {
                Multimethod.call(String.class, "test", "toString", 1); // spurious argument
                Assert.fail("Expected multimethod to fail due to a spurious argument");
            }
            catch (NoMatchingMethodException e)
            {
            }

            try
            {
                Multimethod.call(int.class, 1234, "toString"); // wrong return type
                Assert.fail("Expected multimethod to fail due to a wrong return type");
            }
            catch (NoMatchingMethodException e)
            {
            }

            try
            {
                Multimethod.call(double.class, target, "square", "8"); // wrong argument type
                Assert.fail("Expected multimethod to fail due to a wrong argument type");
            }
            catch (NoMatchingMethodException e)
            {
            }
        }

        /**
         * Tests overloaded calls by using the PrettyPrint class to format a complex data structure
         */
        @Test
        public void callOverloaded()
        {           
            Assert.assertEquals("[INT(1), FLOAT(2.000000), INT(3), ???, STRING(test), " +
            		"[STRING(hello), INT(1234)], DOUBLE(6.000000), STRING(test2)] // test object",
                Multimethod.call(String.class, pp, "print", complicatedObject, "test object")); 
        }

        /**
         * Tests calls to void methods
         */
        @Test
        public void callVoid()
        {
            Multimethod.call(void.class, target, "saySomething");
            Assert.assertEquals("hi", target.getMessage());

            Multimethod.call(void.class, target, "saySomething", "hello");
            Assert.assertEquals("hello", target.getMessage());

            Multimethod.call(void.class, target, "saySomething", 1234);
            Assert.assertEquals("1234", target.getMessage());
        }

        /**
         * Tests calls involving a null argument
         */
        @Test
        public void callWithNull()
        {
            Multimethod.call(void.class, target, "saySomething", new Object[] { null });
            Assert.assertEquals(null, target.getMessage());

            Assert.assertEquals("hi null", Multimethod.call(String.class, target, "append", "hi ", null));
        }

        /**
         * Not a test per se, but a microbenchmark that can be used to test for major
         * performance improvements/degradations.
         */
        @Test
        public void benchmark()
        {
            final int RUNS = 10000;
            long start = System.currentTimeMillis();
            for (int i = 0; i < RUNS; i++)
            {
                Multimethod.call(String.class, pp, "print", complicatedObject);
            }
            double elapsed = System.currentTimeMillis() - start;
            elapsed /= 1000.0; // msec -> sec
            System.out.println(String.format("Microbenchmark operations per second: %f", (double)RUNS / elapsed));
        }
        
    /**
    * Contains some of the methods we'll be testing the Multimethod on.
    * The other methods are in PrettyPrinter.
    */
    class TestTarget
    {
        public String message;

        public String getMessage() {
			return message;
		}

        public void saySomething()
        {
            message = "hi";
        }

        public void saySomething(String msg)
        {
            message = msg;
        }

        public void saySomething(int num)
        {
            message = "" + num;
        }

		public double square(double x)
        {
            return x * x;
        }

        public String append(String a, String b)
        {
            return a + b;
        }
    }
}
