# CRUD-Final - Student Management System



**Author:** Roshjens Kunwar

**Course:** IT Security Management and DevOps



## 1. Project Description



A Student Management CRUD REST API developed using ASP.NET Core and C#. The application uses MariaDB for persistent data storage and is containerized with Docker and deployed to Kubernetes using Minikube.



## 2. Technologies



- C# and ASP.NET Core (.NET 10)

- MariaDB 11.4

- Docker and Docker Compose

- Kubernetes and Minikube

- k6 load testing

- PowerShell



## 3. Application Features



- Create student records

- Retrieve all student records

- Retrieve a student by ID

- Update student records

- Delete student records

- Health monitoring

- StudentHub web interface



### API Endpoints



| Method | Endpoint | Purpose |

|---|---|---|

| POST | `/api/students` | Create a student |

| GET | `/api/students` | Retrieve all students |

| GET | `/api/students/{id}` | Retrieve one student |

| PUT | `/api/students/{id}` | Update a student |

| DELETE | `/api/students/{id}` | Delete a student |

| GET | `/health` | Health check |



## 4. Docker Setup



The multi-stage Dockerfile uses the .NET SDK to build the application and the ASP.NET runtime image to run it.



The application listens on port 8080 and runs as a non-root user.



Docker Compose starts:

- The CRUD application

- MariaDB database

- A named Docker network

- A persistent database volume

- An application health check



Start the services:



```powershell

docker compose up -d --build

```



Check service status:



```powershell

docker compose ps

```



Stop the services:



```powershell

docker compose down

```



Do not use `docker compose down -v` if you want to preserve database data.



## 5. Kubernetes Deployment



Kubernetes manifests are stored in the `k8s/` directory.



| File | Purpose |

|---|---|

| app-deployment.yaml | Application Deployment and probes |

| app-service.yaml | Application NodePort Service |

| configmap.yaml | Application configuration |

| secret.yaml | Development database credentials |

| database-deployment.yaml | MariaDB Deployment |

| database-service.yaml | MariaDB ClusterIP Service |

| database-pvc.yaml | Persistent storage claim |



The application and database run in the `crud-system` namespace.



Apply the manifests:



```powershell

kubectl apply -f .\k8s\

```



Check pods:



```powershell

kubectl get pods -n crud-system

```



Check services:



```powershell

kubectl get services -n crud-system

```



The application uses startup, readiness and liveness probes on `/health`.



## 6. Functional Testing



The following operations were tested successfully:



- POST: Create a student.

- GET: Retrieve student records.

- PUT: Update a student.

- DELETE: Delete a student.

- GET `/health`: Verify application health.



Database persistence was also verified after restarting Docker Compose services.



## 7. Load Testing



Load testing was performed using k6 through Docker.



Test configuration:

- 500 configured virtual users.

- 60 seconds duration.

- GET requests to `/api/students`.

- HTTP status checks.



The successful tests used Kubernetes port forwarding:



```powershell

kubectl port-forward --address 127.0.0.1 service/crud-app 8081:8080 -n crud-system

```



Application URL: `http://127.0.0.1:8081`



### Successful Test Results



| Metric | Test 1 | Test 2 |

|---|---:|---:|

| Configured VUs | 500 | 500 |

| Duration | 60 seconds | 60 seconds |

| HTTP requests | 22,573 | 11,622 |

| Throughput | 370.89 req/s | 186.68 req/s |

| Average response time | 1.32 s | 2.60 s |

| 95th percentile | 1.62 s | 3.88 s |

| Failed HTTP requests | 0% | 0% |

| Successful checks | 100% | 100% |



Results varied between runs. These measurements represent the local Docker and Minikube environment and are not production performance guarantees.



JSON summaries and the k6 test script are stored in `loadtest/`.



## 8. Resource Monitoring



Resource usage was monitored using:



```powershell

kubectl top pods -n crud-system

```



Highest recorded samples during the resource-monitoring test:



| Pod | CPU | Memory |

|---|---:|---:|

| CRUD application | 15m | 79Mi |

| MariaDB | 22m | 119Mi |



These are the highest collected samples, not guaranteed peak usage.



Process inspection reported 18 application threads. This is the total process thread count, not a dedicated Kestrel worker count. ASP.NET Core uses asynchronous request handling and the .NET ThreadPool.



## 9. Fault-Tolerance Test



The application process was deliberately terminated inside its Kubernetes pod.



```powershell

kubectl exec crud-app-5dbb696b8-2wddm -n crud-system -- sh -c "kill 1"

```



Observed results:



- Restart count before failure: 0.

- Restart count after failure: 1.

- Application pod returned to Running.

- Pod readiness returned to 1/1.

- `/health` returned `Healthy` after recovery.



This demonstrated automatic container recovery by Kubernetes.



## 10. Horizontal Pod Autoscaling



No Horizontal Pod Autoscaler (HPA) is currently deployed.



Proposed future scaling policy:



- Minimum replicas: 1.

- Maximum replicas: 5.

- Target average CPU utilization: 70% of the CPU request.

- Current application CPU request: 100m per pod.

- Proposed scale-up threshold: sustained average CPU above 70m per pod.



These are proposed settings, not an active autoscaling configuration.



## 11. Repository Structure



```text

CRUD-Final/
|-- k8s/
|-- loadtest/
|-- report/
|-- src/
|   |-- CrudApp/
|       |-- wwwroot/
|       |-- Program.cs
|       |-- CrudApp.csproj
|-- .gitignore
|-- Dockerfile
|-- docker-compose.yml
|-- README.md

```



## 12. Security Notes



The database credentials in the development configuration are not production secrets. They should be replaced with securely managed credentials before production deployment.



Generated .NET `bin/` and `obj/` directories are excluded from Git using `.gitignore`.
