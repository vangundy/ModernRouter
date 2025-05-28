# CODE REVIEW CHECKLIST
## Implementation
- Does this code change do what it is supposed to do?
- Can this solution be simplified?
- Does this change add unwanted compile-time or run-time dependencies?
- Was a framework, API, library, service used that should not have been used?
- Was a framework, API, library, service not used that could improve the solution?
- Is the code at the right abstraction level?
- Is the code modular enough?
- Would you have solved the problem in a different way that is substantially better in terms of the code’s maintainability, readability, performance, security?
- Does similar functionality already exist in the codebase? If so, why isn’t this functionality reused?
- Are there any best practices, design patterns or language-specific patterns that could substantially improve this code?
- Does this code follow Object-Oriented Analysis and Design Principles, like the Single Responsibility Principle, Open close Principle, Liskov Substitution Principle, Interface Segregation, Dependency Injection? (SOLID)

## Dependencies
- If this change requires updates outside of the code, like updating the documentation, configuration, readme files, was this done?
- Might this change have any ramifications for other parts of the system, backward compatibility?

## Security and Data Privacy
- Does this code open the software for security vulnerabilities? 
- Are authorization and authentication handled in the right way?
- Is sensitive data securely handled and stored?
- Is the right encryption used?
- Does this code change reveal some secret information like keys, passwords, or usernames?
- If code deals with user input, does it address security vulnerabilities such as cross-site scripting, SQL injection, does it do input sanitization and validation? Is data retrieved from external APIs or libraries checked accordingly?

## Logic Errors and Bugs
- Can you think of any use case in which the code does not behave as intended?
- Can you think of any inputs or external events that could break the code?

## Error Handling and Logging
- Is error handling done the correct way?
- Should any logging or debugging information be added or removed?
- Are error messages user-friendly?
- Are there enough log events and are they written in a way that allows for easy debugging?

## Readability
- Was the code easy to understand? Which parts were confusing to you and why?
- Can the readability of the code be improved by smaller methods?
- Can the readability of the code be improved by different function/method or variable names?
- Is the code located in the right file/folder/package?
- Do you think certain methods should be restructured to have a more intuitive control flow?
- Is the data flow understandable? 
- Are there redundant comments?
- Could some comments convey the message better?
- Would more comments make the code more understandable?
- Could some comments be removed by making the code itself more readable? 
- Is there any commented out code?

## Performance
- Do you think this code change will impact system performance in a negative way?
- Do you see any potential to improve the performance of the code?

## Usability
- Is the proposed solution well designed from a usability perspective?
- Is the UI intuitive to use?
- Is the API intuitive to use?
- Is the API well documented? 

## Accessibility
- Is the proposed solution (UI) accessible? (Only public facing APIs)

## Testing and Testability
- Is the code testable?
- Does it have enough automated tests (unit/integration/system tests)? 
- Do the existing tests reasonably cover the code change?
- Are there some test cases, input or edge cases that should be tested in addition?

## Best Practices & Design Patterns
### Early Return Principle
Return from a method as soon as the conditions for that have been met. Sometimes this means moving from a deeply nested structure to a set of guard clauses.
### Merge If Statements to Improve Readability
### Replace Boolean Expression with Descriptive Method
### Prefer Throwing Custom Exceptions
Use exceptions for "exceptional" situations only, and do not use them for flow control. Having said that, if you do want to use exceptions for flow control, it is better to use custom exceptions.
You can introduce valuable contextual information and better describe the reason for throwing the exception. And if you want to handle these exceptions globally, you can create a base class to be able to catch specific exceptions.
### Fix Magic Numbers with Constants
### Fix Magic Strings with Enums
### Use The Result Object Pattern

