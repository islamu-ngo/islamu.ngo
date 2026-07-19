# ABOUTME: Builds the networkless-compatible production environment validation image.
# ABOUTME: Uses BusyBox and copies only the bounded validator entrypoint.
FROM busybox:1.37.0

COPY --chmod=0555 validate-production-environment.sh /opt/islamu/validate-production-environment.sh

ENTRYPOINT ["/bin/sh", "/opt/islamu/validate-production-environment.sh"]
