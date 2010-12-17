package multipledispatch;

import java.lang.reflect.Method;
import java.lang.reflect.Type;
import java.util.Arrays;
import java.util.Collection;

/**
 * Uniquely identifies a method.
 * 
 * @author "Maciej Pacula (maciej.pacula@gmail.com)"
 * 
 */
public class Signature {
	public String name;
	public Type targetType;
	public Collection<Class<?>> parameterTypes;
	public Type returnType;

	public String getName() {
		return name;
	}

	public Type getTargetType() {
		return targetType;
	}

	public Collection<Class<?>> getParameterTypes() {
		return parameterTypes;
	}

	public Type getReturnType() {
		return returnType;
	}

	public Signature(Type targetType, String name, Type returnType,
			Collection<Class<?>> parameterTypes) {
		this.name = name;
		this.targetType = targetType;
		this.parameterTypes = parameterTypes;
		this.returnType = returnType;
	}

	public Signature(Method m) {
		name = m.getName();
		targetType = m.getDeclaringClass();
		returnType = m.getReturnType();
		parameterTypes = Arrays.asList(m.getParameterTypes());
	}

	@Override
	public boolean equals(Object obj) {
		if (!(obj instanceof Signature))
			return false;

		Signature o = (Signature) obj;
		return targetType.equals(o.targetType) && name.equals(o.name)
				&& parameterTypes.equals(parameterTypes)
				&& returnType.equals(o.returnType);
	}

	@Override
	public int hashCode() {
		int hash = 17 * returnType.hashCode();
		hash = 19 * hash + targetType.hashCode();
		hash = 307 * hash + name.hashCode();

		int elt_hash = 1;
		for (Class<?> t : parameterTypes) {
			elt_hash = 31 * elt_hash + (t == null ? 0 : t.hashCode());
		}

		return hash + elt_hash;
	}

	@Override
	public String toString() {
		String msg = String.format("%s %s (", returnType.toString(), name);
		boolean first = true;
		for (Class<?> param : parameterTypes) {
			if (!first)
				msg += ", ";
			msg += (param == null ? "null" : param.getName());
			if (first)
				first = false;
		}
		msg += ")";
		return msg;
	}
}