# Project Instructions

## Description
This project is designed to test your knowledge of **back-end web technologies**, specifically in **.NET**, and assess your ability to create back-end products with attention to **details, standards, and reusability**.

## Assignment
The goal of this exercise is to create a **simple browser-based chat application** using .NET.  
This application should allow multiple users to chat in a chatroom and also retrieve **stock quotes** from an API using a specific command.

## Mandatory Features
- **User Login:** Allow registered users to log in and chat with other users in a chatroom.  
- **Stock Command:** Allow users to post messages as commands into the chatroom using the following format:  /stock=stock_code

- **Decoupled Bot:**  
- Call an API using the `stock_code` as a parameter:  
  `https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv` (here `aapl.us` is the stock code)  
- Parse the received CSV file and send a message back into the chatroom using a **message broker** like RabbitMQ.  
- The message format should be:  
  ```
  AAPL.US quote is $93.42 per share
  ```  
- The post owner should be the bot.  
- **Message Ordering:** Show messages ordered by timestamps and display only the **last 50 messages**.  
- **Unit Tests:** Test at least one functionality of your choice.

## Bonus (Optional)
- Support **multiple chatrooms**.  
- Use **.NET Identity** for user authentication.  
- Handle messages that are **not understood** or any exceptions raised by the bot.  
- Build an **installer**.

## Considerations
- Tests will involve opening **2 browser windows** and logging in with 2 different users.  
- **Stock commands will not be saved** in the database as a post.  
- Project is **backend-focused**; keep the frontend **as simple as possible**.  
- Keep **confidential information secure**.  
- Monitor resource usage to avoid excessive consumption.  
- Keep your code **versioned with Git** locally.  
- Small helper libraries may be used if needed.