using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Istio
{
    [Command("install", Description = "Install istio")]
    internal class IstioInstallCommand
    {
        private readonly string _script = @"#!/bin/bash
curl -L https://istio.io/downloadIstio | ISTIO_VERSION=1.15.0 TARGET_ARCH=x86_64 sh -

cd istio-1.15.0

./bin/istioctl install -f {istio.yaml} -y

kubectl create ns bookinfo
kubectl label namespace bookinfo istio-injection=enabled
kubectl apply -f samples/bookinfo/platform/kube/bookinfo.yaml -n bookinfo

kubectl apply -f samples/addons/prometheus.yaml
kubectl apply -f samples/addons/grafana.yaml
kubectl apply -f samples/addons/kiali.yaml
kubectl apply -f samples/addons/jaeger.yaml

kubectl apply -f {istio-gateway.yaml}
kubectl apply -f {bookinfo-vs.yaml}
kubectl apply -f {prometheus-vs.yaml}
kubectl apply -f {grafana-vs.yaml}
kubectl apply -f {kiali-vs.yaml}
kubectl apply -f {jaeger-query.yaml}
kubectl apply -f {jaeger-vs.yaml}";

        private readonly string _istioYaml = @"
apiVersion: install.istio.io/v1alpha1
kind: IstioOperator
metadata:
  namespace: istio-system
spec:
  meshConfig:
    enableTracing: true
    defaultConfig:
      tracing:
        sampling: 100
        zipkin:
          address: jaeger-collector.istio-system:9411
    enablePrometheusMerge: true

  components:
    # Istio Gateway feature
    ingressGateways:
    - name: istio-ingressgateway
      enabled: true
      k8s:
        service:
          ports:
          - name: status-port
            nodePort: 30021
            port: 15021
            protocol: TCP
            targetPort: 15021
          - name: http2
            nodePort: 30080
            port: 80
            protocol: TCP
            targetPort: 8080
          - name: https
            nodePort: 30443
            port: 443
            protocol: TCP
            targetPort: 8443";

        private readonly string _gatewayYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: Gateway
metadata:
  name: istio-gateway
  namespace: istio-system
spec:
  selector:
    istio: ingressgateway
  servers:
  - hosts:
    - '*.example.com'
    port:
      name: http
      number: 80
      protocol: HTTP";

        private readonly string _bookinfoVsYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: bookinfo-vs
  namespace: bookinfo
spec:
  hosts:
  - 'bookinfo.example.com'
  gateways:
  - istio-system/istio-gateway
  http:
  - match:
    - uri:
        exact: /productpage
    - uri:
        prefix: /static
    - uri:
        exact: /login
    - uri:
        exact: /logout
    - uri:
        prefix: /api/v1/products
    route:
    - destination:
        host: productpage.bookinfo.svc.cluster.local
        port:
          number: 9080";

        private readonly string _prometheusVsYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: prometheus-vs
  namespace: istio-system
spec:
  hosts:
    - 'prometheus.example.com'
  gateways:
    - istio-system/istio-gateway
  http:
    - match:
        - uri:
            prefix: /
      route:
        - destination:
            host: prometheus.istio-system.svc.cluster.local
            port:
              number: 9090";

        private readonly string _grafanaVsYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: grafana-vs
  namespace: istio-system
spec:
  hosts:
    - 'grafana.example.com'
  gateways:
    - istio-system/istio-gateway
  http:
    - match:
        - uri:
            prefix: /
      route:
        - destination:
            host: grafana.istio-system.svc.cluster.local
            port:
              number: 3000";

        private readonly string _kialiVsYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: kiali-vs
  namespace: istio-system
spec:
  hosts:
    - 'kiali.example.com'
  gateways:
    - istio-system/istio-gateway
  http:
    - match:
        - uri:
            prefix: /
      route:
        - destination:
            host: kiali.istio-system.svc.cluster.local
            port:
              number: 20001";

        private readonly string _jaegeQuerySvcYaml = @"
apiVersion: v1
kind: Service
metadata:
  name: jaeger-query
  namespace: istio-system
  labels:
    app: jaeger
spec:
  type: ClusterIP
  ports:
  - name: http
    port: 16686
    targetPort: 16686
    protocol: TCP
  selector:
    app: jaeger";

        private readonly string _jaegerVsYaml = @"
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: jaeger-vs
  namespace: istio-system
spec:
  hosts:
    - 'jaeger.example.com'
  gateways:
    - istio-system/istio-gateway
  http:
    - match:
        - uri:
            prefix: /
      route:
        - destination:
            host: jaeger-query.istio-system.svc.cluster.local
            port:
              number: 16686";

        [Option(Description = "Domain, eg: example.com", ShortName = "d")]
        [Required]
        public string Domain { get; set; }

        public void OnExecute(IConsole console, ICommandService commandService)
        {
            var defaultDomain = "example.com";

            var istioYamlPath = Path.Combine(IstioCommand.Workspace, "istio.yaml");
            File.WriteAllText(istioYamlPath, _istioYaml);

            var istioGatewayYamlPath = Path.Combine(IstioCommand.Workspace, "istio-gateway.yaml");
            File.WriteAllText(istioGatewayYamlPath, _gatewayYaml.Replace(defaultDomain, Domain));

            var bookinfoVsYamlPath = Path.Combine(IstioCommand.Workspace, "bookinfo-vs.yaml");
            File.WriteAllText(bookinfoVsYamlPath, _bookinfoVsYaml.Replace(defaultDomain, Domain));

            var prometheusVsYamlPath = Path.Combine(IstioCommand.Workspace, "prometheus-vs.yaml");
            File.WriteAllText(prometheusVsYamlPath, _prometheusVsYaml.Replace(defaultDomain, Domain));

            var grafanaVsYamlPath = Path.Combine(IstioCommand.Workspace, "grafana-vs.yaml");
            File.WriteAllText(grafanaVsYamlPath, _grafanaVsYaml.Replace(defaultDomain, Domain));

            var kialiVsYamlPath = Path.Combine(IstioCommand.Workspace, "kiali-vs.yaml");
            File.WriteAllText(kialiVsYamlPath, _kialiVsYaml.Replace(defaultDomain, Domain));

            var jaegerQuerySvcYamlPath = Path.Combine(IstioCommand.Workspace, "jaeger-query.yaml");
            File.WriteAllText(jaegerQuerySvcYamlPath, _jaegeQuerySvcYaml);

            var jaegerVsYamlPath = Path.Combine(IstioCommand.Workspace, "jaeger-vs.yaml");
            File.WriteAllText(jaegerVsYamlPath, _jaegerVsYaml.Replace(defaultDomain, Domain));

            var path = Path.Combine(Program.Workspace, "install-istio.sh");
            File.WriteAllText(path, _script.Replace("{istio.yaml}", istioYamlPath)
                                           .Replace("{istio-gateway.yaml}", istioGatewayYamlPath)
                                           .Replace("{bookinfo-vs.yaml}", bookinfoVsYamlPath)
                                           .Replace("{prometheus-vs.yaml}", prometheusVsYamlPath)
                                           .Replace("{grafana-vs.yaml}", grafanaVsYamlPath)
                                           .Replace("{kiali-vs.yaml}", kialiVsYamlPath)
                                           .Replace("{jaeger-query.yaml}", jaegerQuerySvcYamlPath)
                                           .Replace("{jaeger-vs.yaml}", jaegerVsYamlPath));

            var code = commandService.ExecuteCommand($"sed -i 's/\r$//' {path}");
            if (code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }

            code = commandService.ExecuteCommand($"bash {path}");

            if (code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }

            console.WriteLine("Install success");
        }
    }
}
