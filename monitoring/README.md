# Monitoring

This setup collects host and container metrics with Prometheus and displays them in Grafana.

## Services

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
- Node exporter: http://localhost:9100
- cAdvisor: http://localhost:8087

Default Grafana credentials are `admin` / `admin`.

## Prometheus Targets

Open http://localhost:9090/targets and check that these targets are `UP`:

- `prometheus`
- `node-exporter`
- `cadvisor`

## Grafana Dashboard

Grafana is provisioned automatically with:

- Prometheus datasource
- `SOA Observability` dashboard in the `SOA` folder

The dashboard shows:

- host CPU usage
- host RAM usage
- host filesystem usage
- host network traffic
- container CPU usage
- container RAM usage
- container filesystem usage
- container network traffic
