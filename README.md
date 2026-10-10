# Smart Format

A Windows utility that lets users "format" a USB stick without wiping their personal files. It scans a selected USB drive letter, selectively deletes hidden system files, caches, malware residues, or junk folders, while explicitly preserving user data.

## Features

- **Smart Drive Detection**: Automatically lists only removable USB drives
- **Selective Cleaning**: Deletes system files, junk folders, and hidden caches
- **Data Preservation**: Choose to preserve Images, Documents, and/or Audio/Video files
- **Safety Guardrails**: 
  - Only allows removable drives
  - Hard-blocks OS drive (C:\) selection
  - Double confirmation before execution
- **Real-time Progress**: Visual progress bar during scanning and cleaning

## System Requirements

- **Windows 10 or later** (recommended)
- **Windows 7/8.1**: Requires [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe)

## Building from Source

```bash
dotnet build -c Release
```

## Running the Application

### Option 1: Download from GitHub Actions
1. Go to the [Actions tab](https://github.com/jamestheakston/smartformat/actions)
2. Select the latest workflow run
3. Download the `SmartFormat-Executable` artifact
4. Extract and run `SmartFormat.exe`

### Option 2: Build locally
```bash
dotnet run
```

## Usage

1. Plug in your USB drive
2. Select the drive from the dropdown (click Refresh if needed)
3. Check the file types you want to preserve (Images, Documents, Audio/Video)
4. Click "Smart Format"
5. Confirm the operation
6. Wait for the cleaning to complete

## Safety Features

- **Drive Type Verification**: Only `Removable` drives are allowed
- **OS Drive Protection**: C:\ drive is explicitly blocked
- **Confirmation Dialog**: Double confirmation before any deletion
- **Cancellable**: Safe cancellation during operation

## What Gets Deleted

- Hidden system files ($RECYCLE.BIN, System Volume Information, etc.)
- Junk folders (.Trashes, .Spotlight-V100, etc.)
- Malware residues and temporary files
- Desktop.ini, Thumbs.db, .DS_Store

## What Gets Preserved

Based on your selections:
- **Images**: .jpg, .png, .gif, .bmp, .tiff, .webp, .raw, etc.
- **Documents**: .doc, .docx, .pdf, .txt, .xls, .xlsx, .ppt, etc.
- **Audio/Video**: .mp3, .mp4, .avi, .mkv, .mov, .wav, .flac, etc.

## Troubleshooting

### "api-ms-win-crt-string-l1-1-0.dll is missing"
This error occurs on Windows 7/8.1. Install the [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe) and try again.

### No drives showing in dropdown
- Ensure your USB drive is properly connected
- Click the "Refresh" button to rescan for drives
- Try reconnecting the USB drive

## License

This project is provided as-is for educational and utility purposes.
