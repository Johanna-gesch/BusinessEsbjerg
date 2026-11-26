# BusinessEsbjerg Holiday Planner

## Security Policies
We make use of both authentictaion and authorization in our application.

### Authentication
To make sure that passwords aren't saved as plain text in our database, which would be unsafe and make them vulnarable 
to being stolen, we use Argon2id to safely hash our passwords. We also generate a random salt for each password, which 
is stored with the hash.  
When a user logs in, the password they entered is hashed again using the stored salt. Then it is compared to 
the stored hash with CryptographicOperations.FixedTimeEquals to avoid timing-based comparisons.

#### JWT
We use JSON Web Tokens for authentication. When a user logs in, the backend generates a token containing the users 
id, username and role. We set a JWT secret key through an environment variable. This is used to compare and 
validate the tokens generated at login.  
After 1 hour the token expires, and you have to log in again to be able to use the endpoints that require 
authentication.

### Authorization
The application uses role based authorization. The endpoints used to manage users (create, edit and delete) require 
admin authorization. Viewing, approving or declining holiday requests require accountant authorization.

## Current state of project

### What works

### What doesn't work

## Link for deployed system

### Swagger

### Client

## Testing strategy
We have chosen to use a test-last approach rather that TDD.  
Since this is a relatively large project with several interconnected components, 
we wanted to focus on implementing the functionality and understanding how the 
different parts worked together, before testing that it works.  
As we are still developing our experience with TDD, we felt a test-last approach would allow us to work more naturally 
with the implementation while still ensuring that the functionality was properly tested afterwards.

We use three different types of tests in our application:

- Unit tests
- Integration tests
- End-to-end tests

Using all of these allows us to verify the system in multiple ways, and it helps us identify different errors rather 
quickly.

We chose to setup GitHub Actions pretty early in the process, since we thought it was a good way of making sure 
everything newly made actually works as it should.

## Github Project

## Sustainability

## Further work
If we were to continue working on this project, we would implement more authorization checks. For example to ensure  
that users can only view certain information about themselves and not other users' information.  

We would also implement an automatic logout after 1 hour when the token expires, so that the user doesn't get confused 
about why the applictaion suddenly stops working.

## AI disclosure
AI tools have been used throughout the development of this project. The tools used by the group include ChatGPT, Claude, 
Microsoft Copilot and OpenAI Codex (only for styling).  
Our primary use of AI has been as a sparring partner for problem-solving, learning and improving our code. The overall 
architecture and implementation choices has been decided by ourselves.  
AI generated suggestions and code have been critically evaluated and adapted to fit our project's requirements.

### AI-generated files and code
We haven't been using AI to create complete files in the backend. In the frontend, a few smaller components have been 
created by AI, but then usually adapted later anyways.  
AI was used more often to generate small code snippets and more difficult methods, which we have always reviewed and 
often adjusted to work like we wanted.  
AI has been particularly helpful when implementing more complex logic and the more difficult tests (integration and 
end-to-end). We used it to help generate test methods based on our existing methods.  
Additionally, OpenAI Codex was used to do the styling of our application, but only that. We made sure it only created 
and edited an index.css file, and only changed our markup if it were style relevant (like classnames).

### AI for feedback and code review
AI has frequently been used as a sparring partner when discussing potential solutions and evaluating existing 
implementations.  
AI suggestions have never been accepted automatically. We often have long discussions with it to find the best solution 
and to learn and understand what it means.

### AI for brainstorming
AI was used in the beginning to figure out how the project should work and to learn a bit more about the earned holiday
days og hours. We used it to give us examples of which entities we should create.

### AI for text generation
AI has been used to assist with ideas, and language optimisation of the README documentation, although it was all 
written by ourselves. Same with code comments, AI was used for ideas to help the understanding of the code, but 
everything was adjusted and written to our own liking.
