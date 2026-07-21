import {
  ArrowRightIcon,
  BellIcon,
  CheckCircleIcon,
  MagnifyingGlassIcon,
  PlusIcon,
  UploadSimpleIcon,
} from '@phosphor-icons/react'

import { Badge } from '../components/ui/badge'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { Input } from '../components/ui/input'

const colorTokens = [
  ['Foreground', 'bg-foreground'],
  ['Primary', 'bg-primary'],
  ['Accent', 'bg-accent'],
  ['Muted', 'bg-muted'],
  ['Border', 'bg-border'],
]

function Section({ eyebrow, title, children }) {
  return (
    <section className="grid gap-6 border-t border-border py-10 md:grid-cols-[220px_1fr]">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">{eyebrow}</p>
        <h2 className="mt-2 text-lg font-semibold tracking-tight">{title}</h2>
      </div>
      <div>{children}</div>
    </section>
  )
}

function DesignSystemPage() {
  return (
    <main className="min-h-screen bg-background">
      <header className="border-b border-border">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4 lg:px-8">
          <div className="flex items-center gap-3">
            <div className="grid size-8 place-items-center rounded-md bg-foreground text-xs font-bold text-white">LH</div>
            <span className="text-sm font-semibold">Loyalty Hub Admin</span>
            <Badge variant="secondary">Design system</Badge>
          </div>
          <Button variant="ghost" size="icon" aria-label="Notifications">
            <BellIcon weight="bold" />
          </Button>
        </div>
      </header>

      <div className="mx-auto max-w-6xl px-6 py-14 lg:px-8">
        <div className="max-w-3xl">
          <Badge variant="outline">Foundation 01</Badge>
          <h1 className="mt-5 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">
            Clear, restrained interfaces for operational work.
          </h1>
          <p className="mt-5 max-w-2xl text-base leading-7 text-muted-foreground">
            A monochrome admin foundation with a deep blue accent for primary actions, focus, and active states.
          </p>
        </div>

        <div className="mt-14">
          <Section eyebrow="01 / Foundation" title="Color tokens">
            <div className="grid gap-3 sm:grid-cols-5">
              {colorTokens.map(([label, color]) => (
                <div key={label} className="rounded-lg border border-border p-2">
                  <div className={`h-20 rounded-md border border-black/5 ${color}`} />
                  <p className="px-1 pb-1 pt-3 text-xs font-medium">{label}</p>
                </div>
              ))}
            </div>
          </Section>

          <Section eyebrow="02 / Actions" title="Buttons">
            <div className="flex flex-wrap items-center gap-3">
              <Button><PlusIcon data-icon="inline-start" weight="bold" />Create voucher</Button>
              <Button variant="secondary">Secondary</Button>
              <Button variant="outline"><UploadSimpleIcon data-icon="inline-start" />Upload banner</Button>
              <Button variant="ghost">Ghost</Button>
              <Button variant="destructive">Delete</Button>
              <Button size="icon" aria-label="Continue"><ArrowRightIcon weight="bold" /></Button>
            </div>
          </Section>

          <Section eyebrow="03 / Forms" title="Inputs">
            <div className="grid max-w-2xl gap-5 sm:grid-cols-2">
              <label className="grid gap-2 text-sm font-medium">
                Voucher name
                <Input placeholder="Summer reward" />
                <span className="text-xs font-normal text-muted-foreground">Use a short, recognizable name.</span>
              </label>
              <label className="grid gap-2 text-sm font-medium">
                Search
                <div className="relative">
                  <MagnifyingGlassIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
                  <Input className="pl-9" placeholder="Search voucher definitions" />
                </div>
              </label>
            </div>
          </Section>

          <Section eyebrow="04 / Status" title="Badges and cards">
            <div className="grid gap-5 lg:grid-cols-2">
              <Card>
                <CardHeader>
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <CardTitle>Voucher definition</CardTitle>
                      <CardDescription>Reusable surface for admin records.</CardDescription>
                    </div>
                    <Badge variant="success"><CheckCircleIcon weight="fill" />Active</Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex items-end justify-between rounded-lg bg-muted p-4">
                    <div>
                      <p className="text-xs text-muted-foreground">Remaining stock</p>
                      <p className="mt-1 text-2xl font-semibold tracking-tight">1,240</p>
                    </div>
                    <Button variant="outline" size="sm">View detail</Button>
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-foreground text-white">
                <CardHeader>
                  <Badge className="w-fit bg-white/10 text-white">Dark surface</Badge>
                  <CardTitle className="pt-3 text-xl">Use dark blue sparingly.</CardTitle>
                  <CardDescription className="text-white/60">
                    Reserve accent color for intent, selection, and clear interaction feedback.
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Button className="bg-white text-foreground hover:bg-white/90">Review tokens<ArrowRightIcon data-icon="inline-end" /></Button>
                </CardContent>
              </Card>
            </div>
          </Section>
        </div>
      </div>
    </main>
  )
}

export { DesignSystemPage }
