"""Generate original illustrated room plates, protagonist frames, and quiet audio beds."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import math
import random
import struct
import wave

ROOT = Path(__file__).resolve().parents[1]
ROOM_DIR = ROOT / "Assets" / "Resources" / "QuietHours" / "Rooms"
CHAR_DIR = ROOT / "Assets" / "Resources" / "QuietHours" / "Character"
AUDIO_DIR = ROOT / "Assets" / "Resources" / "QuietHours" / "Audio"
W, H = 1920, 1080
FLOOR_Y = 875
SCALE = 3

ROOM_DIR.mkdir(parents=True, exist_ok=True)
CHAR_DIR.mkdir(parents=True, exist_ok=True)
AUDIO_DIR.mkdir(parents=True, exist_ok=True)

FONT_SANS = r"C:\Windows\Fonts\bahnschrift.ttf"
FONT_SERIF = r"C:\Windows\Fonts\georgia.ttf"
FONT_MONO = r"C:\Windows\Fonts\consola.ttf"

P = {
    "ink": (23, 29, 28),
    "deep": (31, 42, 39),
    "sage": (61, 77, 67),
    "sage_light": (91, 101, 83),
    "paper": (217, 207, 177),
    "paper_dim": (177, 170, 147),
    "brass": (170, 132, 83),
    "warm": (210, 166, 105),
    "rust": (139, 81, 62),
    "blue": (88, 112, 117),
    "shadow": (17, 22, 22),
}


def font(path, size):
    try:
        return ImageFont.truetype(path, size)
    except OSError:
        return ImageFont.load_default()


def tracked(draw, xy, text, fnt, fill, spacing=3):
    x, y = xy
    for ch in text:
        draw.text((x, y), ch, font=fnt, fill=fill)
        x += draw.textlength(ch, font=fnt) + spacing


def draw_center(draw, xy, text, fnt, fill):
    x, y = xy
    box = draw.textbbox((0, 0), text, font=fnt)
    draw.text((x - (box[2] - box[0]) / 2, y), text, font=fnt, fill=fill)


def gradient(base, top=None):
    top = top or tuple(max(0, c - 11) for c in base)
    im = Image.new("RGB", (W, H), base)
    px = im.load()
    for y in range(FLOOR_Y):
        t = y / max(1, FLOOR_Y)
        c = tuple(int(top[k] * (1 - t) + base[k] * t) for k in range(3))
        for x in range(W):
            px[x, y] = c
    return im


def soft_glow(im, x, y, radius, color, alpha=90):
    layer = Image.new("RGBA", im.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    d.ellipse((x - radius, y - radius * .78, x + radius, y + radius * .78), fill=(*color, alpha))
    layer = layer.filter(ImageFilter.GaussianBlur(radius * .55))
    im.paste(Image.alpha_composite(im.convert("RGBA"), layer).convert("RGB"))


def paper_lines(draw, x, y, width, count, gap=17, color=(105, 103, 91)):
    for i in range(count):
        end = width * (0.72 + ((i * 13) % 25) / 100)
        draw.line((x, y + i * gap, x + end, y + i * gap), fill=color, width=2)


def door(draw, x, label, brass=False, left=False):
    y0, y1, width = 316, FLOOR_Y, 224
    x0 = x if not left else x - width
    x1 = x0 + width
    frame = (103, 91, 68) if brass else (77, 91, 79)
    draw.rectangle((x0 - 17, y0 - 16, x1 + 17, y1 + 4), fill=(34, 43, 39))
    draw.rectangle((x0 - 7, y0 - 8, x1 + 7, y1), fill=frame)
    # Keep the frame and identification plaque, but leave the passage open so
    # adjacent illustrated rooms correspond to traversable space in play.
    draw.rectangle((x0 + 8, y0 + 9, x1 - 8, y1 - 10), fill=(17, 24, 23), outline=(45, 57, 51), width=3)
    draw.rectangle((x0 + 12, y1 - 18, x1 - 12, y1 - 10), fill=(72, 70, 56))
    draw.rectangle((x0 + 24, y0 - 62, x1 - 24, y0 - 24), fill=(29, 36, 34), outline=(126, 116, 88), width=2)
    draw_center(draw, (x0 + width / 2, y0 - 53), label, font(FONT_MONO, 15), (206, 193, 158))
    draw.line((x0 - 11, y0 - 14, x0 - 11, y1), fill=(165, 137, 89), width=4)


def base_room(name, palette, light_positions):
    im = gradient(palette[0], palette[1])
    # Local pools of warm light; the rest of the room remains readable and quiet.
    for x, y, radius, color, alpha in light_positions:
        soft_glow(im, x, y, radius, color, alpha)
    d = ImageDraw.Draw(im)
    # Plaster panels, repeated but not perfectly identical.
    d.rectangle((0, 0, W, 152), fill=(28, 35, 34))
    d.line((0, 151, W, 151), fill=(118, 111, 86), width=3)
    d.rectangle((0, 153, W, 205), fill=(37, 47, 43))
    d.line((0, 207, W, 207), fill=(91, 98, 80), width=2)
    for x in range(70, W, 280):
        d.line((x, 210, x, FLOOR_Y - 10), fill=(69, 79, 67), width=2)
        d.line((x + 8, 210, x + 8, FLOOR_Y - 10), fill=(48, 60, 54), width=2)
    d.rectangle((0, 755, W, FLOOR_Y), fill=(50, 60, 52))
    for x in range(0, W, 310):
        d.rectangle((x, 762, x + 4, FLOOR_Y - 5), fill=(89, 92, 73))
    d.rectangle((0, FLOOR_Y, W, H), fill=(47, 45, 39))
    # Perspective floorboards, seam marks, and soft scuffs.
    for y in range(FLOOR_Y + 27, H, 70):
        d.line((0, y, W, y - 10), fill=(68, 64, 54), width=2)
    for x in range(80, W, 190):
        offset = (x * 7) % 110
        d.line((x + offset, FLOOR_Y + 5, x + offset - 40, H), fill=(59, 56, 48), width=2)
    d.line((0, FLOOR_Y, W, FLOOR_Y - 7), fill=(166, 141, 93), width=5)
    # The viewer reads the room from the side: wall, baseboard, and floor remain continuous.
    door(d, 192, "RETURN", left=True)
    door(d, 1727, "NEXT", brass=True)
    d.rectangle((0, 0, 21, H), fill=(22, 29, 27))
    d.rectangle((W - 21, 0, W, H), fill=(22, 29, 27))
    tracked(d, (55, 57), name, font(FONT_MONO, 20), (177, 169, 139), 4)
    return im


def draw_sconce(draw, x, y, warm=True):
    metal = (118, 112, 90)
    glow = (219, 176, 112) if warm else (145, 171, 169)
    draw.line((x, y - 15, x, y + 31), fill=metal, width=6)
    draw.rectangle((x - 20, y + 23, x + 20, y + 52), fill=(46, 51, 44), outline=metal, width=3)
    draw.ellipse((x - 10, y + 29, x + 10, y + 48), fill=glow)
    draw.line((x - 35, y + 57, x + 37, y + 57), fill=(93, 90, 73), width=2)


def room_corridor():
    im = base_room("NORTH CORRIDOR     /     LEVEL 01", ((55, 69, 61), (39, 52, 48)), [(350, 450, 400, (183, 155, 105), 58), (1270, 430, 310, (182, 159, 117), 52)])
    d = ImageDraw.Draw(im)
    # Long ceiling beams and practical fluorescent housings, some switched off.
    for x in (430, 980, 1510):
        d.rectangle((x - 95, 165, x + 95, 190), fill=(34, 42, 38), outline=(97, 101, 83), width=3)
        d.line((x - 71, 177, x + 61, 177), fill=(209, 188, 145) if x != 980 else (75, 84, 73), width=5)
    # Corkboard and a worn transit notice.
    d.rectangle((380, 302, 736, 559), fill=(76, 57, 43), outline=(140, 119, 82), width=8)
    d.rectangle((397, 319, 719, 543), fill=(139, 116, 79))
    d.rectangle((423, 342, 678, 510), fill=(210, 200, 171), outline=(86, 77, 58), width=2)
    tracked(d, (441, 357), "TRANSFER / 05:30", font(FONT_MONO, 18), (59, 66, 57), 2)
    paper_lines(d, 444, 399, 210, 4, 23, (123, 119, 99))
    d.line((454, 492, 625, 492), fill=(146, 98, 64), width=3)
    # A simple bench with a folded coat: ordinary evidence of a night shift.
    d.rectangle((916, 660, 1390, 704), fill=(72, 55, 42), outline=(141, 117, 82), width=3)
    d.rectangle((947, 704, 977, 823), fill=(44, 43, 37))
    d.rectangle((1326, 704, 1356, 823), fill=(44, 43, 37))
    d.polygon([(1090, 652), (1148, 623), (1228, 638), (1266, 662), (1240, 682), (1111, 678)], fill=(83, 98, 86), outline=(42, 49, 43))
    # Wall clock, not centered like a quest marker.
    d.ellipse((1462, 333, 1553, 424), fill=(31, 38, 35), outline=(170, 142, 91), width=5)
    d.ellipse((1471, 342, 1544, 415), fill=(190, 180, 149))
    d.line((1508, 379, 1508, 352), fill=(49, 53, 44), width=4)
    d.line((1508, 379, 1528, 389), fill=(49, 53, 44), width=4)
    for deg in range(0, 360, 30):
        x1 = 1508 + math.cos(math.radians(deg)) * 29
        y1 = 379 + math.sin(math.radians(deg)) * 29
        x2 = 1508 + math.cos(math.radians(deg)) * 33
        y2 = 379 + math.sin(math.radians(deg)) * 33
        d.line((x1, y1, x2, y2), fill=(87, 79, 61), width=2)
    # Plant and umbrella stand break up the corridor's strict lines.
    d.rectangle((153, 700, 249, 796), fill=(74, 59, 47), outline=(147, 122, 81), width=4)
    for ox, oy in ((190, 655), (151, 636), (220, 623), (168, 601), (232, 588)):
        d.ellipse((ox - 37, oy - 23, ox + 28, oy + 28), fill=(60, 86, 66), outline=(39, 59, 48), width=3)
    draw_sconce(d, 790, 322, warm=False)
    return grain(im, 1, 21)


def room_records():
    im = base_room("RECORDS OFFICE     /     EAST WING", ((83, 76, 58), (57, 55, 47)), [(660, 460, 470, (208, 165, 102), 88), (1390, 510, 250, (174, 129, 79), 45)])
    d = ImageDraw.Draw(im)
    # Tall storage drawers and labeled cabinets.
    for cx in (390, 1440):
        d.rectangle((cx - 128, 292, cx + 128, 751), fill=(61, 53, 43), outline=(139, 117, 79), width=5)
        for row in range(5):
            y = 314 + row * 81
            d.rectangle((cx - 106, y, cx + 106, y + 69), fill=(83, 72, 55), outline=(123, 103, 73), width=2)
            d.rectangle((cx - 42, y + 16, cx + 43, y + 39), fill=(187, 170, 131), outline=(71, 61, 48), width=2)
            d.line((cx - 11, y + 50, cx + 12, y + 50), fill=(173, 145, 91), width=3)
            if row in (1, 3):
                d.line((cx - 75, y + 8, cx - 48, y + 8), fill=(152, 129, 87), width=2)
    # Broad work table, visually grounded with braces and drawers.
    d.polygon([(550, 641), (1196, 641), (1242, 681), (523, 681)], fill=(114, 83, 56), outline=(47, 43, 36))
    d.rectangle((583, 680, 1162, 708), fill=(77, 60, 44))
    d.rectangle((595, 708, 633, 835), fill=(50, 47, 40))
    d.rectangle((1100, 708, 1138, 835), fill=(50, 47, 40))
    d.rectangle((692, 690, 1006, 754), fill=(84, 66, 49), outline=(140, 112, 74), width=3)
    for y in (704, 724, 744):
        d.line((705, y, 992, y), fill=(134, 105, 70), width=2)
    # A lamp creates the warm pool; its shade remains visibly practical.
    d.line((1098, 638, 1098, 527), fill=(38, 42, 37), width=9)
    d.line((1098, 527, 1154, 477), fill=(38, 42, 37), width=8)
    d.polygon([(1110, 449), (1188, 451), (1215, 503), (1094, 503)], fill=(170, 130, 79), outline=(60, 52, 41))
    d.ellipse((1100, 495, 1211, 555), fill=(172, 133, 82))
    # Open ledger and carbon copy: content is present without making every line readable at a distance.
    d.polygon([(702, 550), (882, 514), (931, 601), (746, 629)], fill=(220, 210, 180), outline=(92, 78, 56))
    d.polygon([(882, 514), (1034, 535), (1014, 623), (931, 601)], fill=(198, 189, 163), outline=(92, 78, 56))
    paper_lines(d, 723, 562, 139, 4, 14, (130, 122, 100))
    paper_lines(d, 944, 550, 68, 5, 13, (125, 119, 101))
    d.line((897, 523, 924, 603), fill=(131, 106, 72), width=3)
    # Mug ring and an old stamped transfer label.
    d.ellipse((608, 584, 672, 608), outline=(110, 80, 58), width=3)
    d.rectangle((1178, 734, 1316, 785), fill=(192, 178, 143), outline=(92, 77, 56), width=2)
    tracked(d, (1191, 749), "E-17 / CARBON", font(FONT_MONO, 12), (73, 70, 57), 1)
    draw_sconce(d, 847, 269, warm=True)
    return grain(im, 2, 26)


def room_lab():
    im = base_room("SIGNAL LAB     /     MEASUREMENT BAY", ((54, 69, 67), (38, 49, 49)), [(970, 406, 340, (186, 175, 130), 63), (1320, 602, 290, (127, 154, 151), 43)])
    d = ImageDraw.Draw(im)
    # Acoustic wall panels and two receiver mounts.
    for x in range(315, 1625, 165):
        tone = (63, 76, 72) if (x // 165) % 2 == 0 else (51, 66, 65)
        d.rectangle((x, 251, x + 134, 504), fill=tone, outline=(98, 105, 87), width=3)
        for yy in range(270, 491, 24):
            d.line((x + 10, yy, x + 122, yy + 2), fill=(tone[0] + 6, tone[1] + 4, tone[2] + 3), width=2)
    # Chalk / plotting board with a hand-drawn two-arrival waveform.
    d.rectangle((545, 259, 1324, 538), fill=(35, 45, 43), outline=(153, 132, 92), width=5)
    tracked(d, (577, 280), "TRIAL 17  /  TWO-RECEIVER RETURN", font(FONT_MONO, 17), (209, 198, 163), 2)
    d.line((605, 454, 1272, 454), fill=(126, 136, 122), width=2)
    d.line((628, 329, 628, 472), fill=(126, 136, 122), width=2)
    pts = [(650, 451), (730, 449), (778, 450), (824, 446), (856, 418), (879, 472), (902, 437), (938, 450), (992, 449), (1080, 450), (1132, 448), (1156, 429), (1177, 466), (1198, 446), (1250, 450)]
    d.line(pts, fill=(198, 173, 122), width=4, joint="curve")
    d.line((878, 361, 1157, 361), fill=(133, 162, 154), width=2)
    d.line((878, 352, 878, 369), fill=(133, 162, 154), width=3)
    d.line((1157, 352, 1157, 369), fill=(133, 162, 154), width=3)
    draw_center(d, (1018, 334), "16 ms (after alignment)", font(FONT_MONO, 13), (163, 176, 158))
    # Two metal transducers mounted at a measured distance.
    for x, label in ((410, "A  /  WEST"), (1531, "B  /  EAST")):
        d.rectangle((x - 51, 533, x + 51, 658), fill=(77, 83, 73), outline=(164, 146, 105), width=4)
        d.ellipse((x - 28, 555, x + 28, 611), fill=(34, 42, 39), outline=(177, 154, 108), width=4)
        d.ellipse((x - 11, 572, x + 11, 594), fill=(150, 161, 143))
        d.rectangle((x - 76, 666, x + 76, 682), fill=(52, 58, 52))
        draw_center(d, (x, 698), label, font(FONT_MONO, 12), (195, 180, 143))
        d.line((x, 681, x, 718), fill=(127, 117, 88), width=3)
    # Control desk and analog waveform reader.
    d.polygon([(570, 716), (1385, 716), (1451, 760), (524, 760)], fill=(76, 70, 57), outline=(143, 124, 87))
    d.rectangle((600, 760, 1369, 796), fill=(47, 52, 48))
    d.rectangle((675, 620, 1022, 712), fill=(32, 42, 40), outline=(152, 134, 96), width=4)
    d.rectangle((693, 638, 1004, 688), fill=(61, 88, 81), outline=(98, 118, 98), width=2)
    d.line((709, 665, 760, 665, 782, 645, 804, 681, 830, 654, 864, 665, 927, 665, 956, 647, 979, 672), fill=(203, 184, 139), width=3)
    for x in (1070, 1112, 1154, 1196):
        d.ellipse((x, 637, x + 21, 658), fill=(157, 132, 89), outline=(38, 45, 39), width=2)
    # Temperature slip pinned at eye level.
    d.rectangle((1280, 568, 1489, 668), fill=(210, 200, 172), outline=(106, 92, 68), width=3)
    tracked(d, (1299, 584), "ROOM / 15 C", font(FONT_MONO, 12), (61, 68, 59), 1)
    paper_lines(d, 1298, 614, 161, 3, 15, (123, 116, 96))
    draw_sconce(d, 950, 203, warm=False)
    return grain(im, 3, 20)


def room_calibration():
    im = base_room("CALIBRATION ROOM     /     ACCESS RESTRICTED", ((65, 60, 53), (43, 43, 39)), [(1194, 428, 315, (198, 152, 94), 73)])
    d = ImageDraw.Draw(im)
    # Dense felt panels with a single split where a service hatch interrupts them.
    for x in range(250, 1680, 115):
        tone = (65, 62, 55) if (x // 115) % 2 else (53, 55, 50)
        d.rectangle((x, 250, x + 88, 744), fill=tone, outline=(98, 89, 69), width=2)
        for yy in range(275, 730, 42):
            d.line((x + 9, yy, x + 77, yy), fill=(tone[0] + 4, tone[1] + 4, tone[2] + 4), width=2)
    # Wide acoustic aperture, closed, with a visible rubber seal.
    d.rectangle((464, 316, 988, 759), fill=(31, 37, 35), outline=(161, 131, 91), width=7)
    d.rectangle((485, 337, 967, 737), fill=(50, 52, 46), outline=(83, 90, 77), width=4)
    for x in range(510, 954, 37):
        d.line((x, 352, x, 722), fill=(62, 67, 58), width=3)
    # Service-call bell and small wire that disappears under the lower trim.
    d.rectangle((1122, 423, 1433, 674), fill=(45, 50, 46), outline=(165, 132, 89), width=5)
    d.rectangle((1142, 445, 1411, 546), fill=(55, 65, 59), outline=(107, 103, 80), width=3)
    tracked(d, (1170, 463), "SERVICE LINE", font(FONT_MONO, 15), (196, 182, 144), 2)
    d.ellipse((1236, 566, 1321, 651), fill=(116, 70, 53), outline=(188, 138, 91), width=5)
    d.ellipse((1257, 585, 1300, 628), fill=(192, 117, 73), outline=(78, 55, 43), width=3)
    d.line((1365, 676, 1365, 794, 1324, 835), fill=(100, 76, 51), width=5)
    # A tape measure between floor marks; the relation is in the files, not printed as a code.
    d.line((296, 812, 1608, 812), fill=(187, 158, 103), width=4)
    for i, x in enumerate(range(306, 1609, 109)):
        d.line((x, 801, x, 824 if i % 2 else 835), fill=(203, 179, 126), width=3)
    d.rectangle((564, 817, 1100, 858), fill=(47, 45, 39))
    tracked(d, (614, 826), "WEST       /       12.00 m       /       EAST", font(FONT_MONO, 14), (177, 164, 129), 2)
    # A low cabinet, open binder, and folded cotton gloves.
    d.rectangle((1041, 697, 1547, 748), fill=(90, 69, 51), outline=(154, 123, 80), width=4)
    d.rectangle((1080, 748, 1111, 846), fill=(51, 47, 40))
    d.rectangle((1474, 748, 1505, 846), fill=(51, 47, 40))
    d.polygon([(1217, 679), (1301, 661), (1363, 700), (1285, 722)], fill=(213, 204, 179), outline=(86, 73, 54))
    paper_lines(d, 1232, 685, 59, 3, 8, (122, 115, 96))
    d.polygon([(909, 693), (982, 671), (1018, 696), (969, 729), (925, 717)], fill=(175, 171, 148))
    draw_sconce(d, 1187, 205, warm=True)
    return grain(im, 4, 29)


def grain(im, amount=2, seed=0):
    rng = random.Random(seed)
    # Restrained pigment grain keeps large areas from reading as flat blocks.
    px = im.load()
    for _ in range((W * H) // 130):
        x = rng.randrange(W)
        y = rng.randrange(H)
        delta = rng.choice((-amount, -1, 1, amount))
        r, g, b = px[x, y]
        px[x, y] = (max(0, min(255, r + delta)), max(0, min(255, g + delta)), max(0, min(255, b + delta)))
    # Edge vignette, very low strength.
    overlay = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    od = ImageDraw.Draw(overlay)
    od.rectangle((0, 0, W, H), outline=(10, 16, 16, 88), width=34)
    return Image.alpha_composite(im.convert("RGBA"), overlay).convert("RGB")


def character_frame(kind):
    scale = SCALE
    im = Image.new("RGBA", (128 * scale, 208 * scale), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Draw at 3x for a deliberately illustrated silhouette with soft edges.
    def q(v):
        return int(round(v * scale))
    ink = (25, 29, 28, 255)
    coat = (74, 91, 80, 255)
    coat_hi = (117, 120, 96, 255)
    trouser = (45, 47, 43, 255)
    skin = (199, 160, 127, 255)
    bag = (153, 111, 66, 255)
    leg_shift = -7 if kind == 1 else 7 if kind == 2 else 0
    # Back leg / boot.
    d.polygon([(q(54), q(133)), (q(73), q(135)), (q(69 + leg_shift), q(181)), (q(58 + leg_shift), q(184)), (q(50), q(155))], fill=trouser, outline=ink, width=q(2))
    d.polygon([(q(57 + leg_shift), q(179)), (q(70 + leg_shift), q(180)), (q(77 + leg_shift), q(190)), (q(75 + leg_shift), q(196)), (q(48 + leg_shift), q(196)), (q(47 + leg_shift), q(190))], fill=(35, 36, 33, 255), outline=ink, width=q(2))
    # Coat, narrow shoulder and flared hem.
    d.polygon([(q(43), q(68)), (q(68), q(63)), (q(85), q(78)), (q(81), q(109)), (q(93), q(145)), (q(43), q(149)), (q(37), q(111))], fill=coat, outline=ink, width=q(3))
    d.polygon([(q(43), q(117)), (q(87), q(114)), (q(93), q(145)), (q(43), q(149))], fill=(62, 77, 69, 255), outline=ink, width=q(2))
    d.line((q(64), q(75), q(62), q(142)), fill=coat_hi, width=q(2))
    d.line((q(72), q(86), q(67), q(101)), fill=(169, 142, 100, 255), width=q(2))
    # Forward arm and hand.
    d.polygon([(q(72), q(79)), (q(84), q(82)), (q(89), q(115)), (q(81), q(125)), (q(74), q(119)), (q(79), q(98))], fill=coat_hi, outline=ink, width=q(2))
    d.ellipse((q(77), q(116), q(88), q(128)), fill=skin, outline=ink, width=q(2))
    # Messenger strap and small satchel.
    d.line((q(48), q(74), q(78), q(124)), fill=(193, 151, 94, 255), width=q(4))
    d.rounded_rectangle((q(33), q(118), q(52), q(142)), radius=q(3), fill=bag, outline=ink, width=q(2))
    d.line((q(36), q(124), q(49), q(124)), fill=(203, 173, 117, 255), width=q(1))
    # Neck, profile, swept hair and tied bun.
    d.rectangle((q(51), q(55), q(68), q(74)), fill=skin, outline=ink, width=q(2))
    d.ellipse((q(43), q(20), q(78), q(64)), fill=skin, outline=ink, width=q(3))
    d.polygon([(q(44), q(45)), (q(41), q(25)), (q(50), q(11)), (q(69), q(12)), (q(83), q(25)), (q(77), q(32)), (q(66), q(28)), (q(63), q(48)), (q(53), q(62))], fill=(34, 36, 34, 255), outline=ink, width=q(3))
    d.ellipse((q(38), q(28), q(49), q(40)), fill=(34, 36, 34, 255), outline=ink, width=q(2))
    d.ellipse((q(69), q(31), q(73), q(35)), fill=ink)
    d.line((q(77), q(43), q(83), q(45)), fill=(111, 69, 57, 255), width=q(2))
    # Collar and a small copper pin.
    d.polygon([(q(51), q(67)), (q(63), q(75)), (q(70), q(66)), (q(66), q(80))], fill=(190, 169, 127, 255), outline=ink, width=q(2))
    d.ellipse((q(66), q(87), q(71), q(92)), fill=(191, 146, 85, 255))
    # Front leg varies for a simple three-frame walk cycle.
    d.polygon([(q(56), q(137)), (q(74), q(136)), (q(75 - leg_shift), q(181)), (q(64 - leg_shift), q(185)), (q(54), q(159))], fill=trouser, outline=ink, width=q(2))
    d.polygon([(q(63 - leg_shift), q(180)), (q(75 - leg_shift), q(181)), (q(82 - leg_shift), q(190)), (q(80 - leg_shift), q(196)), (q(53 - leg_shift), q(196)), (q(53 - leg_shift), q(190))], fill=(35, 36, 33, 255), outline=ink, width=q(2))
    # Subtle edge lighting keeps the figure legible against dark walls.
    d.line((q(39), q(88), q(37), q(125), q(44), q(139)), fill=(135, 141, 111, 255), width=q(2))
    return im.resize((128, 208), Image.Resampling.LANCZOS)


def write_wav(path, samples, rate=22050):
    with wave.open(str(path), "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(rate)
        wf.writeframes(b"".join(struct.pack("<h", max(-32768, min(32767, int(v * 32767)))) for v in samples))


def ambience(seconds, seed, hum, breath, noise_level, pulse=0.0):
    rate = 22050
    rng = random.Random(seed)
    out = []
    phase = 0.0
    for i in range(int(seconds * rate)):
        t = i / rate
        n = (rng.random() * 2.0 - 1.0) * noise_level
        air = math.sin(2 * math.pi * 0.17 * t + seed) * 0.035
        low = math.sin(2 * math.pi * hum * t) * 0.085
        overtone = math.sin(2 * math.pi * (hum * 1.51) * t + .7) * 0.026
        phase = (phase + 1.0 / rate) % max(.1, pulse) if pulse else 0
        soft_event = math.sin(2 * math.pi * breath * t) * 0.025
        out.append((n + low + overtone + air + soft_event) * 0.55)
    return out


def fx(duration, tones, noise=0.0, decay=8.0, seed=0):
    rate = 22050
    rng = random.Random(seed)
    out = []
    for i in range(int(duration * rate)):
        t = i / rate
        env = math.exp(-decay * t)
        tone = sum(math.sin(2 * math.pi * f * t + phase) * amp for f, amp, phase in tones)
        out.append((tone + (rng.random() * 2 - 1) * noise) * env * 0.28)
    return out


def main():
    rooms = [room_corridor(), room_records(), room_lab(), room_calibration()]
    names = ["Room_Corridor", "Room_Records", "Room_SignalLab", "Room_Calibration"]
    for im, name in zip(rooms, names):
        im.save(ROOM_DIR / (name + ".png"), optimize=True)
    for n, pose in (("Ira_Idle", 0), ("Ira_StepA", 1), ("Ira_StepB", 2)):
        character_frame(pose).save(CHAR_DIR / (n + ".png"), optimize=True)
    beds = [
        ambience(12, 12, 52, 117, .012),
        ambience(12, 19, 47, 173, .010),
        ambience(12, 31, 63, 211, .014),
        ambience(12, 46, 39, 89, .008),
    ]
    for sample, name in zip(beds, ("Ambience_Corridor", "Ambience_Records", "Ambience_SignalLab", "Ambience_Calibration")):
        write_wav(AUDIO_DIR / (name + ".wav"), sample)
    write_wav(AUDIO_DIR / "Step.wav", fx(.19, [(104, .70, 0), (189, .17, .4)], .11, 17, 2))
    write_wav(AUDIO_DIR / "Paper.wav", fx(.42, [(980, .08, 0), (1580, .04, .3)], .34, 6, 5))
    write_wav(AUDIO_DIR / "Terminal.wav", fx(.16, [(440, .42, 0), (660, .20, .5)], .008, 18, 7))
    # A quiet two-note service chime used only when the file is corrected.
    bell = []
    rate = 22050
    for i in range(int(.95 * rate)):
        t = i / rate
        a = math.exp(-7 * t) * math.sin(2 * math.pi * 392 * t)
        b = math.exp(-9 * max(0, t - .19)) * math.sin(2 * math.pi * 523.25 * max(0, t - .19)) if t >= .19 else 0
        bell.append((a + b) * .12)
    write_wav(AUDIO_DIR / "CorrectionChime.wav", bell)
    print(f"Generated {len(rooms)} room plates, 3 character frames, and 9 audio clips.")
    for path in sorted(list(ROOM_DIR.glob("*.png")) + list(CHAR_DIR.glob("*.png")) + list(AUDIO_DIR.glob("*"))):
        print(f"{path.relative_to(ROOT)} {path.stat().st_size:,} bytes")


if __name__ == "__main__":
    main()
