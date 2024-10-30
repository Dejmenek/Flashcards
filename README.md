# Flashcards

## Table of Contents
- [General Info](#general-info)
- [Technologies](#technologies)
- [Features](#features)
- [Examples](#examples)
- [Instalation and Setup](#instalation-and-setup)
- [Requirements](#requirements)
- [Challenges](#challenges)
- [Things Learned](#things-learned)
- [Used Resources](#used-resources)

## General Info
Project made for @TheCSharpAcademy.  
This project is a basic flashcards application written in C# using SQL Server and Dapper for data access.  
It allows users to create stacks, add flashcards with questions and answers, see study sessions statistics, and practice their knowledge.

## Technologies
- C#
- SQL Server
- Dapper
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)

## Features
- Create and manage stacks of flashcards.
- Add flashcards with questions and answers.
- Practise flashcards.
- Review study sessions statistics.
- User-friendly interface: Provides clear menus and prompts for interaction.
- Input validation: Ensure data entered by the user is valid.

## Examples
- Main Menu  
![image](https://github.com/user-attachments/assets/7757b64f-a45c-4001-899d-5753178781a0)
- Manage Stacks  
![image](https://github.com/user-attachments/assets/446c9474-fab7-41dd-8e5e-fa1e78e7e0d0)  
![image](https://github.com/user-attachments/assets/a5be447b-28e6-4663-af36-cbd199ae8ff2)  
- Manage Flashcards  
![image](https://github.com/user-attachments/assets/cb7c62ac-3191-4061-870f-fb2a7d9ec245)  
- Study Session  
![image](https://github.com/user-attachments/assets/459497ca-21bb-45c5-b4cb-4c1dcab611e4)  
- View Study Sessions  
![image](https://github.com/user-attachments/assets/22e3de25-f6e3-4b16-8132-27b183eae03f)  
- View monthly study session reports per stack:
	- Average Score  
![image](https://github.com/user-attachments/assets/0bb9a83d-108a-43b4-895b-10ac0bb2426a)  
	- Number of sessions  
![image](https://github.com/user-attachments/assets/2079e36c-5288-4701-b76e-7f459995f102)

## Instalation and Setup
1. Clone or download this project repository.
2. Open the solution file (Flashcards.Dejmenek.sln) in Visual Studio.
3. Install the required NuGet packages:
	- Dapper
	- System.Data.SqlClient
	- Spectre.Console
	- Spectre.Console.Cli
	- System.Configuration.ConfigurationManager
4. Update the App.config file with your SQL Server connection string details.
  
## Requirements
- [x] This is an application where the users will create Stacks of Flashcards.
- [x] You'll need two different tables for stacks and flashcards. The tables should be linked by a foreign key.
- [x] Stacks should have an unique name.
- [x] Every flashcard needs to be part of a stack. If a stack is deleted, the same should happen with the flashcard.
- [x] You should use DTOs to show the flashcards to the user without the Id of the stack it belongs to.
- [x] When showing a stack to the user, the flashcard Ids should always start with 1 without gaps between them. If you have 10 cards and number 5 is deleted, the table should show Ids from 1 to 9.
- [x] After creating the flashcards functionalities, create a "Study Session" area, where the users will study the stacks. All study sessions should be stored, with date and score.
- [x] The study and stack tables should be linked. If a stack is deleted, it's study sessions should be deleted.
- [x] The project should contain a call to the study table so the users can see all their study sessions. This table receives insert calls upon each study session, but there shouldn't be update and delete calls to it.

## Challenges
- [x] Try to create a report system where you can see the number of sessions per month per stack.
- [x] Try to create a report system where you can see the average score per month per stack.

## Things Learned
I have some previous experience with other DBMS like MySQL and SQLite so using SQL Server wasn't that hard.  
I only had to look up for data types and how to create pivot tables. Unfortunately, I encountered an issue with auto inrecementing ids in tables.  
When inserting a new record, the id would jump to 1000. It was weird, because it only occurred in the flashcards table.  
I removed all the tables and then recreated them. Now it works as intended😅.

I've also learned about Data Transfer Objects (DTOs).  
They provide a clean way to transfer data between different layers of an application, reducing the amount of unnecessary data sent.
In the future, I'd like to develop this project into a full-stack app with more features😁.

## Used Resources
- [SQL Server Tutorial](https://www.sqlservertutorial.net) used to learn some SQL Server syntax
- [C# Corner](https://www.c-sharpcorner.com/article/data-transfer-objects-dtos-in-c-sharp/) used to learn about DTOs
