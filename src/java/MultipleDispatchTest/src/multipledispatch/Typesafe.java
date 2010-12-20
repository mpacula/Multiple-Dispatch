package multipledispatch;

import junit.framework.Assert;

import org.junit.Test;

public class Typesafe {
    @Test
    public void typePrinterTest() {
        TypePrinter typePrinter = Dispatch.using(TypePrinter.class, new TypePrinterImplementation());
        Assert.assertEquals("It's an integer", typePrinter.typeOf(42));
        Assert.assertEquals("It's a string", typePrinter.typeOf("foo"));
        Assert.assertEquals("It's unknown", typePrinter.typeOf(3.14));
    }

    interface TypePrinter {
        public String typeOf(Object value);
    }

    static class TypePrinterImplementation implements TypePrinter {
        public String typeOf(Integer value) {
            return "It's an integer";
        }

        public String typeOf(String value) {
            return "It's a string";
        }

        public String typeOf(Object value) {
            return "It's unknown";
        }
    }    
}

