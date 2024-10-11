# GitHub Action - AdGuardHome DNS rewrites sync

This GitHub Action helps to sync AdGuardHome DNS rewrites


## Usage

Add this step in your workflow file
```yaml
- name: Update AdGuard DNS Rewrites
  uses: A-A-S-DevLab/actions-adguardhome-dns-rewrites-sync@v1.0.0
  with:
      path: ./path/rewrites.json
      username: user
      userpassword: passwors
      url: http://0.0.0.0:80
```

## Input Variables

- `path`: Path to the JSON file or directory with JSON files containing the DNS rewrites (e.g. './path/rewrites.json')
- `username`: username (e.g. 'user')
- `userpassword`: userpassword (e.g. 'password')
- `url`: url (e.g. 'http://0.0.0.0:80')
