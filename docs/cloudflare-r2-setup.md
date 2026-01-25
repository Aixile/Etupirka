# Cloudflare R2 Setup Guide for Etupirka Cloud Sync

## Overview

This guide walks you through setting up Cloudflare R2 storage for syncing Etupirka play time data across multiple machines.

**Cost:** R2 has a generous free tier:
- 10 GB storage/month
- 10 million Class A operations (writes)/month
- 1 million Class B operations (reads)/month
- No egress fees

For Etupirka sync, you'll likely stay well within the free tier.

---

## Step 1: Create a Cloudflare Account

1. Go to [https://dash.cloudflare.com/sign-up](https://dash.cloudflare.com/sign-up)
2. Enter your email and create a password
3. Verify your email address

---

## Step 2: Enable R2 Storage

1. Log in to the Cloudflare dashboard
2. In the left sidebar, click **R2 Object Storage**
3. If prompted, add a payment method (required to enable R2, but you won't be charged within free tier)
4. Click **Create bucket**

---

## Step 3: Create a Bucket

1. **Bucket name:** Enter a unique name, e.g., `etupirka-sync` or `my-etupirka-data`
   - Use lowercase letters, numbers, and hyphens only
   - Must be globally unique across all Cloudflare users

2. **Location:** Choose "Automatic" or select a region close to you:
   - APAC (Asia Pacific) - good for Japan
   - WNAM (Western North America)
   - ENAM (Eastern North America)
   - WEUR (Western Europe)
   - EEUR (Eastern Europe)

3. Click **Create bucket**

---

## Step 4: Create API Token

1. In the left sidebar, click **R2 Object Storage**
2. Click **Manage R2 API Tokens** (in the upper right area)
3. Click **Create API token**

4. Configure the token:
   - **Token name:** `Etupirka Sync` (or any name you prefer)
   - **Permissions:** Select **Object Read & Write**
   - **Specify bucket(s):** Select **Apply to specific buckets only**
   - Choose the bucket you created (e.g., `etupirka-sync`)
   - **TTL:** Leave as "Forever" or set an expiration

5. Click **Create API Token**

6. **IMPORTANT:** Copy and save these values immediately (they won't be shown again):
   - **Access Key ID** (looks like: `a1b2c3d4e5f6...`)
   - **Secret Access Key** (looks like: `abc123def456...`)

---

## Step 5: Get Your R2 Endpoint URL

1. Go back to **R2 Object Storage** in the sidebar
2. Click on your bucket name
3. Click **Settings** tab
4. Find the **S3 API** section
5. Copy the **Endpoint URL** - it looks like:
   ```
   https://<account-id>.r2.cloudflarestorage.com
   ```

Or construct it manually:
- Your Account ID is visible in the URL when viewing R2: `https://dash.cloudflare.com/<account-id>/r2`
- Endpoint format: `https://<account-id>.r2.cloudflarestorage.com`

---

## Step 6: Configure Etupirka

1. Open Etupirka
2. Go to **Settings** (設定)
3. Click the **Sync** (同期) tab
4. Fill in the fields:

| Field | Value |
|-------|-------|
| R2 エンドポイント | `https://<account-id>.r2.cloudflarestorage.com` |
| アクセスキー ID | Your Access Key ID from Step 4 |
| シークレットキー | Your Secret Access Key from Step 4 |
| バケット名 | Your bucket name (e.g., `etupirka-sync`) |
| 同期間隔 | Sync interval in minutes (default: 5) |

5. Click **接続テスト** (Test Connection)
   - Should show **接続成功** (Connection successful) in green

6. Check **クラウド同期を有効にする** (Enable cloud sync)

7. Click **OK** to save

---

## Step 7: Set Up on Additional Machines

Repeat Step 6 on each machine you want to sync:

1. Install Etupirka
2. Configure the **same** R2 credentials (endpoint, keys, bucket)
3. Enable sync

Each machine gets a unique Machine ID automatically. Play time from all machines will be merged.

---

## Troubleshooting

### "接続失敗" (Connection failed)

- Double-check the endpoint URL format
- Verify Access Key ID and Secret Key are correct
- Ensure the bucket name matches exactly (case-sensitive)
- Check your internet connection

### "Access Denied" errors

- Verify the API token has "Object Read & Write" permission
- Ensure the token is scoped to the correct bucket
- Token may have expired if you set a TTL

### Games not syncing

- Both machines must have the game with the **same ErogameScape ID**
- For games without ESID, use **クラウドにリンク** (Link to Cloud) to manually link them

### Sync indicator is orange (error)

- Hover over the indicator to see the error message
- Common issues: network timeout, invalid credentials, bucket deleted

---

## Data Storage Structure

Your R2 bucket will contain:

```
bucket/
├── events/
│   ├── abc123def456.json    # Events from Machine 1
│   ├── xyz789ghi012.json    # Events from Machine 2
│   └── ...
├── games.json               # Shared game metadata
└── links.json               # Manual game links
```

---

## Security Notes

- Your API keys are stored locally in Windows user settings
- Only you have access to your R2 bucket
- Play time data is stored as JSON (not encrypted)
- Consider using a dedicated Cloudflare account for extra isolation

---

## Cost Estimation

Typical Etupirka usage:
- ~1 KB per sync (events JSON)
- Syncing every 5 minutes = ~288 syncs/day
- ~8,640 syncs/month = ~8.6 MB storage + ~17,280 operations

**Well within free tier** (10 GB storage, 10M writes, 1M reads)
