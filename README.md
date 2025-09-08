# Flashcards

## Table of Contents
- [General Info](#general-info)
- [Technologies](#technologies)
- [Features](#features)
- [Examples](#examples)
- [Instalation and Setup](#instalation-and-setup)
- [Things Learned](#things-learned)
- [Used Resources](#used-resources)
- [TODO](#todo)

## General Info
Flashcards is a modern, feature-rich flashcard application built with C# and .NET 9, designed to help users efficiently learn and retain information.  
It supports creating and managing stacks of flashcards with various card types, including basic, multiple choice and cloze deletion.  
The application implements the Leitner System for spaced repetition, allowing users to optimize their study sessions.

## Technologies
- C#
- SQL Server
- Dapper
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)
- Docker
- XUnit
- NSubstitute
- TestContainers

## Features
- Create and manage stacks of flashcards.
- Support for multiple flashcard types: basic, multiple choice and cloze deletion.
- Practise flashcards with spaced repetition using the Leitner System.
- Review study sessions statistics, including monthly reports (average score, number of sessions per stack).
- User-friendly interface: Provides clear menus and prompts for interaction.
- (Planned) Generate flashcards from text files or other formats.

## Instalation and Setup
You can run the Flashcards app either locally or using Docker.

### Local Setup
#### Steps
1. Clone or download this project repository.
2. Open the solution file (Flashcards.Dejmenek.sln) in Visual Studio.
3. Install the required NuGet packages
4. Update the appsettings.json file.
	- Make sure you have two connection strings:
		```
		{
			"ConnectionStrings": {
				"Default": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Flashcards;Integrated Security=True;",
				"Master": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;"
			}
		}
		```
5. Build and Run the application from Visual Studio or CLI.

### Docker

#### Prerequisites
- Docker installed on your machine.
- Docker Compose installed.

#### Steps
1. Clone or download this project repository.
2. Navigate to the project directory in your terminal.
3. Run the following command to build and start the Docker containers:
   ```bash
   docker-compose up -d --build
   ```

#### Accessing the Application
Once the containers are running, you can access the application through the terminal.
You can run the application using the following command:
```bash
docker compose run --service-ports -it flashcards
```

## Examples
- Main Menu  
![image](https://github.com/user-attachments/assets/7757b64f-a45c-4001-899d-5753178781a0)
- Manage Stacks  
![image](https://github.com/user-attachments/assets/446c9474-fab7-41dd-8e5e-fa1e78e7e0d0)  
![image](https://github.com/user-attachments/assets/a5be447b-28e6-4663-af36-cbd199ae8ff2)  
- Manage Flashcards  
![image](https://github.com/user-attachments/assets/cb7c62ac-3191-4061-870f-fb2a7d9ec245)  
- Card Types
  - Basic Card
  - Multiple Choice
  - Cloze Deletion
- Study Session  
![image](https://github.com/user-attachments/assets/459497ca-21bb-45c5-b4cb-4c1dcab611e4)  
- View Study Sessions  
![image](https://github.com/user-attachments/assets/22e3de25-f6e3-4b16-8132-27b183eae03f)  
- View monthly study session reports per stack:
	- Average Score  
![image](https://github.com/user-attachments/assets/0bb9a83d-108a-43b4-895b-10ac0bb2426a)  
	- Number of sessions  
![image](https://github.com/user-attachments/assets/2079e36c-5288-4701-b76e-7f459995f102)

## Things Learned
I have some previous experience with other DBMS like MySQL and SQLite so using SQL Server wasn't that hard.  
I only had to look up for data types and how to create pivot tables.

I've also learned about Data Transfer Objects (DTOs).  
They provide a clean way to transfer data between different layers of an application, reducing the amount of unnecessary data sent.

Working with Docker taught me how to containerize .NET applications and manage dependencies in isolated environments.  
I learned how to write Dockerfiles, use Docker Compose for multi-container setups, and troubleshoot common issues related to networking and persistent storage.

Implementing GitHub Actions introduced me to automating CI/CD workflows.  
I learned how to set up automated builds and tests for pull requests.


## Used Resources
- [SQL Server Tutorial](https://www.sqlservertutorial.net) used to learn some SQL Server syntax
- [C# Corner](https://www.c-sharpcorner.com/article/data-transfer-objects-dtos-in-c-sharp/) used to learn about DTOs
- [The Leitner System](https://subjectguides.york.ac.uk/study-revision/leitner-system) used to learn about spaced repetition

## TODO
- [x] Add unit tests for the application.
- [x] Add integration tests for the application.
- [x] Add more card types (e.g., multiple choice, cloze).
- [x] Leitner System implementation for spaced repetition.
- [ ] Generate flashcards from text files or other formats.
