# Synthesizes host "voice" candidates (gibberish speech sounds) as WAV files. Pure Python, no dependencies.
import math, random, struct, wave, os, sys

SR = 22050
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)


def write(name, samples):
    peak = max(1e-6, max(abs(s) for s in samples))
    gain = 0.85 / peak
    with wave.open(os.path.join(OUT, name), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * gain)) * 32767)) for s in samples))


class Biquad:
    """RBJ band pass (constant peak gain)."""
    def __init__(self, f, q):
        self.set(f, q)
        self.x1 = self.x2 = self.y1 = self.y2 = 0.0

    def set(self, f, q):
        w0 = 2 * math.pi * f / SR
        alpha = math.sin(w0) / (2 * q)
        a0 = 1 + alpha
        self.b0, self.b1, self.b2 = alpha / a0, 0.0, -alpha / a0
        self.a1, self.a2 = -2 * math.cos(w0) / a0, (1 - alpha) / a0

    def __call__(self, x):
        y = self.b0 * x + self.b1 * self.x1 + self.b2 * self.x2 - self.a1 * self.y1 - self.a2 * self.y2
        self.x2, self.x1, self.y2, self.y1 = self.x1, x, self.y1, y
        return y


class LowPass:
    def __init__(self, f):
        self.a = math.exp(-2 * math.pi * f / SR)
        self.y = 0.0

    def __call__(self, x):
        self.y = (1 - self.a) * x + self.a * self.y
        return self.y


VOWELS = {"a": (800, 1200), "e": (500, 1900), "i": (300, 2300), "o": (500, 900), "u": (350, 700)}


def env(t, length, attack=0.012, release=0.03):
    if t < attack:
        return t / attack
    if t > length - release:
        return max(0.0, (length - t) / release)
    return 1.0


def animalese(base, seed, dur=2.6):
    """Cute formant syllables, each with its own vowel and pitch (gibberish talk)."""
    rnd = random.Random(seed)
    out = []
    t = 0.0
    phase = 0.0
    while t < dur:
        syl = rnd.uniform(0.06, 0.095)
        gap = rnd.choice([0.0, 0.0, 0.01, 0.12 if rnd.random() < 0.15 else 0.0])
        v = VOWELS[rnd.choice("aeiou")]
        f1, f2 = Biquad(v[0], 6), Biquad(v[1], 9)
        pitch = base * rnd.uniform(0.9, 1.25)
        n = int(syl * SR)
        for i in range(n):
            tt = i / SR
            f = pitch * (1 + 0.15 * (tt / syl))  # little upward glide
            phase += f / SR
            saw = 2 * (phase % 1.0) - 1
            out.append((f1(saw) * 1.0 + f2(saw) * 0.6) * env(tt, syl))
        out += [0.0] * int(gap * SR)
        t += syl + gap
    return out


def blips(base, seed, dur=2.6):
    """Short square beeps per letter, gaps between words (classic RPG text sound)."""
    rnd = random.Random(seed)
    out = []
    t = 0.0
    lp = LowPass(3500)
    while t < dur:
        word = rnd.randint(2, 7)
        for _ in range(word):
            length = 0.035
            f = base * rnd.choice([1.0, 1.0, 1.12, 0.94])
            n = int(length * SR)
            for i in range(n):
                tt = i / SR
                sq = 1.0 if math.sin(2 * math.pi * f * tt) >= 0 else -1.0
                out.append(lp(sq * 0.5) * env(tt, length, 0.002, 0.01))
            out += [0.0] * int(0.022 * SR)
            t += length + 0.022
        out += [0.0] * int(0.09 * SR)
        t += 0.09
    return out


def robot(base, seed, dur=2.6):
    """Robotic mumble: ring modulated, bit crushed buzz with a syllable rhythm and wobbling pitch."""
    rnd = random.Random(seed)
    out = []
    t = 0.0
    phase = 0.0
    while t < dur:
        syl = rnd.uniform(0.08, 0.16)
        target = base * rnd.uniform(0.85, 1.2)
        v = VOWELS[rnd.choice("aeiou")]
        f1 = Biquad(v[0] * 1.2, 4)
        n = int(syl * SR)
        for i in range(n):
            tt = i / SR
            f = target * (1 + 0.04 * math.sin(2 * math.pi * 7 * tt))
            phase += f / SR
            saw = 2 * (phase % 1.0) - 1
            ring = math.sin(2 * math.pi * 31 * (t + tt))  # metallic ring modulation
            s = f1(saw) * (0.6 + 0.4 * ring)
            s = round(s * 6) / 6  # bit crush
            out.append(s * env(tt, syl, 0.01, 0.02))
        pause = 0.13 if rnd.random() < 0.2 else 0.015
        out += [0.0] * int(pause * SR)
        t += syl + pause
    return out


def wahwah(base, seed, dur=2.6):
    """Muted horn 'wah' syllables: a filter that opens and closes on every syllable."""
    rnd = random.Random(seed)
    out = []
    t = 0.0
    phase = 0.0
    while t < dur:
        syl = rnd.uniform(0.14, 0.26)
        pitch = base * rnd.uniform(0.92, 1.12)
        bp = Biquad(400, 3)
        n = int(syl * SR)
        for i in range(n):
            tt = i / SR
            open_ = math.sin(math.pi * tt / syl)
            if i % 32 == 0:
                bp.set(350 + 1100 * open_ ** 1.5, 3)
            phase += pitch * (1 - 0.06 * (tt / syl)) / SR
            saw = 2 * (phase % 1.0) - 1
            out.append(bp(saw) * env(tt, syl, 0.02, 0.04))
        pause = 0.12 if rnd.random() < 0.25 else 0.03
        out += [0.0] * int(pause * SR)
        t += syl + pause
    return out


def bubbles(base, seed, dur=2.6):
    """Soft rising pops per word: barely there, never in the way."""
    rnd = random.Random(seed)
    out = []
    t = 0.0
    while t < dur:
        length = rnd.uniform(0.05, 0.08)
        f0 = base * 2 * rnd.uniform(0.9, 1.15)
        n = int(length * SR)
        phase = 0.0
        for i in range(n):
            tt = i / SR
            f = f0 * (1 + 1.2 * tt / length)
            phase += f / SR
            out.append(math.sin(2 * math.pi * phase) * math.exp(-tt * 45) * env(tt, length, 0.003, 0.01))
        gap = rnd.uniform(0.09, 0.2)
        out += [0.0] * int(gap * SR)
        t += length + gap
    return out


STYLES = {"a_animalese": animalese, "b_blips": blips, "c_robot": robot, "d_wahwah": wahwah, "e_bubbles": bubbles}
HOSTS = {"konstantin": (110, 11), "ilja": (175, 23)}  # base pitch (Hz), seed

for name, fn in STYLES.items():
    for host, (pitch, seed) in HOSTS.items():
        base = pitch * (3 if name == "b_blips" else 1)
        write(f"voice_{name}_{host}.wav", fn(base, seed, float(os.environ.get("VOICE_SECONDS", "2.6"))))
print("ok", sorted(os.listdir(OUT)))
