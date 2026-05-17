from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image


PIECE_NAMES = ["orange", "gray", "tuxedo", "calico", "black", "white", "bluegray"]


def is_chroma_key(red: int, green: int, blue: int) -> bool:
    return green > 175 and red < 95 and blue < 95 and green - max(red, blue) > 80


def is_chroma_edge(red: int, green: int, blue: int) -> bool:
    return green > 130 and red < 130 and blue < 130 and green - max(red, blue) > 45


def remove_green_background(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    pixels = image.load()
    width, height = image.size

    for y in range(height):
        for x in range(width):
            red, green, blue, alpha = pixels[x, y]
            if is_chroma_key(red, green, blue):
                pixels[x, y] = (red, green, blue, 0)
            elif is_chroma_edge(red, green, blue):
                pixels[x, y] = (red, green, blue, max(0, min(alpha, 90)))

    return image


def center_on_canvas(image: Image.Image, size: int = 256, subject_size: int = 224) -> Image.Image:
    alpha = image.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError("sprite segment does not contain a visible subject")

    subject = image.crop(bounds)
    width, height = subject.size
    scale = min(subject_size / width, subject_size / height)
    new_size = (max(1, int(width * scale)), max(1, int(height * scale)))
    subject = subject.resize(new_size, Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    offset = ((size - new_size[0]) // 2, (size - new_size[1]) // 2)
    canvas.alpha_composite(subject, offset)
    return canvas


def split_sprite_sheet(source: Path, output_dir: Path) -> list[Path]:
    sheet = Image.open(source).convert("RGBA")
    width, height = sheet.size
    segment_width = width / len(PIECE_NAMES)
    output_dir.mkdir(parents=True, exist_ok=True)
    output_paths: list[Path] = []

    for index, name in enumerate(PIECE_NAMES):
        left = int(round(index * segment_width))
        right = int(round((index + 1) * segment_width))
        segment = sheet.crop((left, 0, right, height))
        transparent = remove_green_background(segment)
        piece = center_on_canvas(transparent)
        path = output_dir / f"{name}.png"
        piece.save(path)
        output_paths.append(path)

    return output_paths


def main() -> None:
    parser = argparse.ArgumentParser(description="Split a seven-cat sprite sheet into transparent piece PNG assets.")
    parser.add_argument("source", type=Path)
    parser.add_argument("output_dir", type=Path)
    args = parser.parse_args()

    for path in split_sprite_sheet(args.source, args.output_dir):
        print(f"{path.name} {path.stat().st_size}")


if __name__ == "__main__":
    main()
