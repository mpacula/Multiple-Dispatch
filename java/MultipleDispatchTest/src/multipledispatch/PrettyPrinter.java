package multipledispatch;

/**
 * A simple pretty printer used to test the Multimethod.
 * 
 * @author "Maciej Pacula (maciej.pacula@gmail.com)"
 *
 */
public class PrettyPrinter {
	public String print(String str)
    {
        return String.format("STRING(%s)", str);
    }

    public String print(Object val, String comment)
    {
        return Multimethod.call(String.class, this, "print", val) + " // " + comment;
    }

    public String print(float value)
    {
        return String.format("FLOAT(%f)", value);
    }

    public String print(double value) 
    {
        return String.format("DOUBLE(%f)", value);
    }

    public String print(int value)
    {
        return String.format("INT(%d)", value);
    }

    public String print(Object value)
    {
        return "???";
    }

    public String print(Object[] array)
    {
        String result = "[";

        boolean first = true;
        for (Object obj : array)
        {
            if (!first)
                result += ", ";

            result += Multimethod.call(String.class, this, "print", obj);

            if (first) first = false;
        }

        return result + "]";
    }
}
