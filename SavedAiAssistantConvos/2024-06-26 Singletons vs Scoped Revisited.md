This is an interesting question about dependency injection and service lifetimes in a FunctionApp context. Let's examine arguments for and against avoiding all Singletons, and then I'll provide a recommendation.

Arguments for avoiding all Singletons:

1. Safety: As demonstrated by the bug, using Singletons increases the risk of accidentally holding onto scoped services, leading to subtle and hard-to-debug issues.

2. Consistency: Having a uniform approach (all services as scoped or transient) simplifies the mental model for developers and reduces cognitive load when reasoning about service lifetimes.

3. Minimal performance impact: In a FunctionApp, where each invocation is relatively short-lived, the overhead of recreating services that would have been Singletons is likely negligible.

4. Easier testing: Avoiding Singletons can make unit testing easier, as you don't have to worry about shared state between tests.

5. Better alignment with serverless architecture: FunctionApps are designed to be stateless, and avoiding Singletons aligns better with this principle.

Arguments against avoiding all Singletons:

1. Performance in high-load scenarios: For extremely high-throughput functions, the cumulative effect of recreating services that could be Singletons might become noticeable.

2. Truly stateless services: Some services are genuinely stateless and thread-safe, and making them Singletons could be a valid optimization without risks.

3. Resource management: Some external resources (like database connections) might benefit from being managed as Singletons to limit the number of connections.

4. Established patterns: Singleton is a well-known pattern, and completely avoiding it might be confusing for developers used to traditional dependency injection practices.

5. Potential for unnecessary object creation: For complex objects that are expensive to create and truly stateless, recreating them for each function invocation could be wasteful.

Conclusion and Recommendation:

After considering both sides, I would recommend a nuanced approach:

1. Adopt a "Scoped by Default" policy: Make scoped lifetime the default choice for all services in your FunctionApp. This aligns well with the serverless, stateless nature of Functions and minimizes the risk of the bug you encountered.

2. Allow Singletons with strict criteria: Permit Singleton registration only when all of the following conditions are met:
   a. The service is completely stateless and thread-safe.
   b. The service does not depend on any scoped or transient services.
   c. There's a measurable performance benefit to making it a Singleton.
   d. The decision to make it a Singleton is documented with a clear rationale.

3. Code review process: Implement a code review checklist that includes verifying the lifetime of services, especially checking that Singletons don't depend on scoped services.

4. Use static analysis tools: If possible, implement or use existing static analysis tools that can detect potential lifetime mismatches in your dependency injection setup.

5. Education and documentation: Ensure your team understands the implications of different service lifetimes in the context of FunctionApps. Document your approach and the reasoning behind it.

6. Regular audits: Periodically review your services and their lifetimes to ensure they still meet the criteria and haven't accidentally acquired dependencies that violate the rules.

This approach balances the safety of avoiding Singletons with the flexibility to use them when truly beneficial. It errs on the side of caution while still allowing for optimizations where they make sense. The key is to make conscious, documented decisions about service lifetimes rather than defaulting to Singletons out of habit.