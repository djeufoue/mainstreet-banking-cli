# Mainstreet Banking CLI

A .NET 10 console banking simulator extended to accept everyday English with the OpenAI Chat Completions API. The original simulator executes the commands and keeps the balances in memory. Both accounts start at $0.00 each time you run the program.

## Run

With the .NET 10 SDK installed, run these commands from the repository root.

```sh
dotnet build BankingCli.sln
dotnet run --project BankingCli
```

In Visual Studio 2026 (18.0 or later), open `BankingCli.sln` and press F5. Visual Studio 2022 users can run the app from a terminal using the .NET 10 SDK.

Set the following environment variables before starting the app or launching Visual Studio. Restart Visual Studio after changing Windows user variables.

| Variable | Value |
| --- | --- |
| `OPENAI_API_KEY` | Your own API key with permission to call Chat Completions. Keep it outside Git. |
| `OPENAI_API_ENDPOINT` | Optional full HTTPS endpoint. Defaults to `https://api.openai.com/v1/chat/completions`. |
| `OPENAI_MODEL` | Optional Chat Completions model. Defaults to `gpt-4.1-mini`. |

An API account with available quota is needed for English requests. The original commands work without an API key. If an English request is missing information, the app asks you to try again with the full request. If the API cannot explain an executed command, the app prints the original simulator result without executing the command again.

## English examples

```text
Deposit ten dollars into savings.
Move four dollars from savings to chequing.
Take one dollar out of chequing.
How much is in my savings account?
What are my balances in both accounts?
```

## Commands

| Command | Effect |
| --- | --- |
| `deposit <account> <amount>` | Add money to an account. |
| `withdraw <account> <amount>` | Take money out of an account. |
| `transfer <from> <to> <amount>` | Move money between accounts. |
| `balance [account]` | Show one balance or both balances when the account is omitted. |
| `help` or `?` | Show the command list. |
| `exit` | End the session. |

Accounts are `chequing` and `savings`. Amounts are positive, with up to two decimal places, such as `5` or `5.25`. Withdrawals and transfers require sufficient funds, and transfers require two different accounts. A failed command leaves both balances as they were.

```text
bank> deposit savings 10
DEPOSIT $10.00 TO savings | savings=$10.00
bank> transfer savings chequing 4
TRANSFER $4.00 FROM savings TO chequing | savings=$6.00 | chequing=$4.00
bank> withdraw chequing 1
WITHDRAW $1.00 FROM chequing | chequing=$3.00
bank> balance
chequing=$3.00 | savings=$6.00
bank> exit
EXIT
```

## Verification

I ran the following checks in one console session with the OpenAI API after seeding savings using `deposit savings 10`. Responses can vary in wording.

| Check | Steps and observed result |
| --- | --- |
| Successful transfer | Entered `move four dollars from savings to chequing`. The reply confirmed the transfer and reported $6 in savings and $4 in chequing. The direct `balance` command confirmed both values. |
| Clarification | Entered `Transfer four dollars from savings`. The app asked for the destination account. |
| Declined withdrawal | Entered `Take twenty dollars out of chequing`. The app explained that the $4.00 available could not cover the $20 withdrawal. |

Following the clarification and decline, English balance queries reported $6.00 for savings alone, then $4.00 in chequing and $6.00 in savings together. The balances still matched the values after the successful transfer.

With simulated API responses, we also verified that an HTTP failure during the rewrite or a reply containing an incorrect amount produced the original transfer output. In each case, the ending balances were $6.00 in savings and $4.00 in chequing after one transfer.

## Development notes

- I used OpenAI Codex to review the starter, implement the API integration in stages, and check the fallback behavior. I ran the live scenarios above in Visual Studio and compared the replies with the account balances.
- We kept the original simulator in charge of all operations. The model proposes one command and explains the simulator's result. The app checks the proposed command before executing it.
- After a live transfer fell back to the raw output, we changed the reply check to compare equivalent dollar formats such as `$4` and `$4.00`. A simulated response confirmed that `$4` was accepted and `$400` was rejected. A live retest produced a conversational reply with the correct balances.
- The reply check compares account names and amounts. It does not independently verify every relationship or claim in the generated wording.
