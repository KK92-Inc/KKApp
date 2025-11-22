# sv

Everything you need to build a Svelte project, powered by [`sv`](https://github.com/sveltejs/cli).

## Creating a project

If you're seeing this, you've probably already done this step. Congrats!

```sh
# create a new project in the current directory
npx sv create

# create a new project in my-app
npx sv create my-app
```

## Developing

Once you've created a project and installed dependencies with `npm install` (or `pnpm install` or `yarn`), start a development server:

```sh
npm run dev

# or start the server and open the app in a new browser tab
npm run dev -- --open
```

## Building

To create a production version of your app:

```sh
npm run build
```

You can preview the production build with `npm run preview`.

> To deploy your app, you may need to install an [adapter](https://svelte.dev/docs/kit/adapters) for your target environment.

```
src/routes/
├ (auth)/                  # Authentication group
│ ├ login/
│ ├ register/
│ └ +layout.svelte
│
├ (app)/                   # Main app routes
│ ├ +layout.svelte         # Common app layout
│ │
│ ├ users/
│ │ ├ +page.svelte         # List all users
│ │ └ [userId]/
│ │   ├ +page.svelte       # User profile
│ │   │
│ │   ├ projects/
│ │   │ ├ +page.svelte     # List user projects
│ │   │ ├ new/
│ │   │ └ [projectId]/
│ │   │   ├ +page.svelte   # View project
│ │   │   └ edit/
│ │   │
│ │   ├ curis/             # Similar structure as projects
│ │   └ goals/             # Similar structure as projects
│ │
│ ├ evaluations/
│ │ ├ +page.svelte
│ │ └ [evaluationId]/
│ │   ├ +layout@(app).svelte  # Unique evaluation layout
│ │   └ +page.svelte
│ │
│ ├ reviews/
│ │ ├ +page.svelte
│ │ └ [reviewId]/
│ │   └ +page.svelte
│ │
│ ├ search/
│ │ └ +page.svelte
│ │
│ └ notifications/
│   └ +page.svelte
│
├ settings/                # Settings section
│ ├ +layout.svelte
│ ├ profile/
│ ├ account/
│ └ preferences/
│
└ +layout.svelte           # Root layout
```