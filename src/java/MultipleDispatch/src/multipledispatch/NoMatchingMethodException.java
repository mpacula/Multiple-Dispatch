package multipledispatch;

/**
 * Thrown by the Multimethod whenever no method can be matched to arguments.
 * 
 * @author "Maciej Pacula (maciej.pacula@gmail.com)"
 */
public class NoMatchingMethodException extends RuntimeException {
	private static final long serialVersionUID = -6818566028119818868L;
	private Signature signature;

	public NoMatchingMethodException(Signature sig) {
		super(composeMessage(sig));
		signature = sig;
	}

	private static String composeMessage(Signature sig) {
		return String.format("No matching method for type signature %s", sig);
	}

	/**
	 * Returns signature of the most specific method that would have matched the
	 * arguments, which is just the requested result type + types of arguments
	 * 
	 * @return signature of the most specific method that would have matched the
	 *         arguments
	 */
	public Signature getSignature() {
		return signature;
	}
}