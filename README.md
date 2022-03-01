# No Russian

```bash
docker build --no-cache -t norussian .
docker run -d --name norussian --env Throttle=0 norussian
docker logs norussian -f
```

Env variables:

- `RetryDelay`
- `RequestTimeout`
- `Threads`
- `Throttle`
