```
▄▄███ ▄▄▄ ▄▄███ ▄▄▄ ▄▄███▄▄▄▄ ▄▄███▄▄▄▄ ▄▄███▄▄▄▄
 ▒███ ░██  ▒███ ░██  ▒███ ░██  ▒███ ░██  ▒███ ░██
 ░███▀▀█▄  ░███▀▀█▄  ░███▀▀██  ░███▄▄█▀  ░███▄▄█▀
 ████ ░██  ████ ░██  ████ ░██   ███       ███    
 ░███ ░██  ░███ ░██  ░███ ░██  ░███      ░███    
 ▀▀▀▀ ░██  ▀▀▀▀ ░██  ▀▀▀  ░██  ▀▀▀       ▀▀▀     
      ▀▀▀       ▀▀▀       ▀▀▀
```

A distributed peer-to-peer learning platform built with .NET Aspire, featuring a modern SvelteKit frontend, .NET Web API backend, and custom Git hosting infrastructure.

> [!CAUTION]
> This project is unlicensed, unfinished and still work in progress. It is quite literally unusable at it's current stage.

## Local

To run this locally you will require podman or docker, however podman is preferred.
Make sure to install aspire as it is used to orchestrate the containers.

Then run:
```bash
aspire run dev
```

If you mess up something you can run:
```bash
./Tools/wipe.sh
```

> [!CAUTION]
> Wipe will literally destroy every volume, every container and quite literally wipe everything away, that is the point.

In the aspire dashboard provide your credentials, for keycloak you can define the secrets there as they should be.
Keycloak will correctly set the secrets up amongst the different clients.

If you don't have a secret like for Resend, fill it with garbage or get a secret.

## The idea

The core philosphy of the peer to peer model in education works but it is not currently used to it's fullest potential. To do so we have to implement various self determination theories into the app itself to allow students to have:
- `Autonomy`: The need to feel in control of your own actions, goals, and choices rather than controlled by external pressures.
- `Competence`: The need to feel effective, master tasks, and build expertise in your environment.
- `Relatedness`: The need to experience a sense of belonging, connection, and care for and from others.

Most alternative platforms do not possess this or encourage any of it and/or ignore it. What you get is the same iteration for a solution with zero improvement just recycled solutions.

Most campuses currently do some of these things but still progress feels like pressing the gas pedal and the break at the same time.

---

# TODO: Student Management

Create a page dedicated to manage users.
Staff only and you basically can edit their display name, email and etc
Then you can manage their profile picture

As well as handling Student role so for example:
- Applicant
- Student
- Alumni (?)
- Staff

Finally you can also enroll them into a cursus:
- You need to have your piscine cursus ready as well as student cursus
- Then you simply designate them to the right cursus.

Needs to be bulk editable somehow we need a proper data table:
- Deactivate
- Anonymize
- Alumnize 
---

You should be able to own the platform you run your education on, be free to modify and not be coerced into a perpetual licensing fee.

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.

See [LICENSE](LICENSE) for details.
