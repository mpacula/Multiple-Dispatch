package multipledispatch;

import java.lang.reflect.*;

public class Dispatch implements InvocationHandler {
    @SuppressWarnings("unchecked")
    public static <T> T using(Class<T> interfaceClass, T implementation) {
        return (T) Proxy.newProxyInstance(
            implementation.getClass().getClassLoader(),
            new Class[] {interfaceClass},
            new Dispatch(implementation));
    }

    private final Object implementation;

    private Dispatch(Object implementation) {
        this.implementation = implementation;
    }

    public Object invoke(Object proxy, Method method, Object[] arguments) throws Throwable {
        return Multimethod.call(method.getReturnType(), implementation, method.getName(), arguments);
    }
}

