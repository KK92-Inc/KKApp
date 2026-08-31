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

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.

See [LICENSE](LICENSE) for details.
