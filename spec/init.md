# Deluxxe: Racing Event Prize Raffle System

## High-Level Overview

Deluxxe is a comprehensive prize raffle management system designed for motorsports racing events. The system conducts automated, fair, and transparent prize drawings for racing participants based on their participation, sponsor sticker compliance, and eligibility criteria.

## Core Purpose

The system automates the process of conducting prize raffles at racing events (specifically focused on BMW PRO3 racing series) where:
- Sponsors provide prizes (gift cards, cash, products) for distribution to race participants
- Winners are selected randomly from eligible participants based on race participation, sponsor sticker requirements, and fairness rules
- Results are tracked, validated, and documented for transparency and compliance

## Key Components

### 1. **Command Line Interface (DeluxxeCli)**
- Primary interface for running raffles and validating driver data
- Supports two main commands:
  - `raffle`: Execute a complete raffle drawing for an event
  - `validate-drivers`: Verify driver eligibility and data consistency
- Configuration-driven operation using JSON files

### 2. **Core Library (Deluxxe)**
- **RaffleService**: Orchestrates the entire raffle process across multiple rounds
- **PrizeRaffle**: Handles individual prize drawings with eligibility checking
- **RaceResultsService**: Fetches and processes race results from SpeedHive API
- **StickerManager**: Manages sponsor sticker compliance tracking
- **PrizeLimitChecker**: Enforces seasonal and event prize limits

## Data Flow & Process

### Input Data Sources
1. **Race Results**: Retrieved from SpeedHive racing timing system via API
2. **Car-to-Sticker Mapping**: CSV files tracking which cars display which sponsor stickers
3. **Prize Descriptions**: JSON files defining available prizes from sponsors
4. **Previous Results**: Historical raffle results to prevent duplicate winnings
5. **Event Configuration**: JSON files defining event parameters, conditions, and settings

### Core Process Flow
1. **Data Validation**: Verify all input data integrity and completeness
2. **Eligibility Filtering**: Apply race conditions (e.g., "PRO3 class", "!DNS" status)
3. **Prize Distribution**:
   - **Per-Race Prizes**: Awarded after each individual race session
   - **Per-Event Prizes**: Awarded once per race weekend
4. **Sticker Compliance**: Only drivers displaying required sponsor stickers are eligible
5. **Fairness Rules**: Prevent duplicate winnings within the same session/event
6. **Random Selection**: Conduct fair random drawings from eligible participant pools
7. **Results Generation**: Output comprehensive results in CSV and JSON formats

### Output Artifacts
- **CSV Results**: Human-readable prize winner lists
- **JSON Results**: Machine-readable structured data
- **Audit Trail**: Complete resource IDs for tracking and verification
- **Summary Reports**: Prize distribution statistics and unclaimed prizes

## Business Rules & Logic

### Eligibility Criteria
- Must participate in race (not DNS - Did Not Start)
- Must display required sponsor sticker on vehicle
- Cannot win same prize type within session (configurable)
- Respects seasonal prize limits per sponsor
- Can optionally filter rental car participants

### Prize Types & Sponsors
- **Primary Sponsors**: 425 Motorsports, Advanced Auto Fabrication, Bimmerworld, Griots Garage, Red Line Oil, Racer on Rails, Toyo Tires, Alpinestars, Proformance
- **Prize Categories**: Gift cards, cash prizes, product vouchers, specialty items (e.g., Toyo tire sets)
- **Value Ranges**: Typically $50-$500 per prize

### Randomization & Fairness
- Cryptographically secure random number generation
- Optional seeded randomization for reproducible testing
- Multi-round drawing system to maximize prize distribution
- Automatic history clearing when no eligible candidates remain

## Technical Architecture

### Technology Stack
- **.NET 9.0**: Primary development framework
- **C#**: Programming language
- **System.CommandLine**: CLI framework
- **JSON/CSV**: Data formats
- **SpeedHive API**: External race results integration

### Key Design Patterns
- **Dependency Injection**: Comprehensive IoC container usage
- **Activity Source**: Distributed tracing and observability
- **Strategy Pattern**: Pluggable sticker management and result writers
- **Command Pattern**: CLI operation handling
- **Factory Pattern**: HTTP client and service creation

### Data Management
- **File-based Configuration**: Local JSON and CSV files
- **URI Abstraction**: Supports both local files and remote URLs
- **Versioned Schemas**: Support for evolving data formats (v1.0, v1.2)
- **Validation Framework**: Comprehensive input data validation

## Event Management Workflow

### Pre-Event Setup
1. Update car-to-sticker mapping for current participants
2. Update prize descriptions from sponsor commitments
3. Create event directory structure
4. Configure event template with race session details
5. Link previous results for historical tracking

### Event Execution
1. Fetch race results from SpeedHive timing system
2. Validate driver eligibility and sticker compliance
3. Execute per-race prize drawings for each session
4. Execute per-event prize drawings for weekend prizes
5. Generate comprehensive results and reports

### Post-Event Processing
1. Export results to CSV for sponsor distribution
2. Store JSON results for future reference
3. Update historical database for future events
4. Generate summary statistics and audit reports

## Integration Points

### External Systems
- **SpeedHive**: Race timing and results system
- **Email Systems**: Results distribution (via templates)
- **Sponsor Systems**: Prize fulfillment coordination

### Output Formats
- **CSV**: Human-readable results for manual processing
- **JSON**: Structured data for system integration
- **Email Templates**: Automated notification generation
- **Audit Logs**: Compliance and verification tracking

## Configuration Management

### Event Templates
- Standardized JSON configuration for consistent setup
- Environment-specific settings (local vs. production)
- Flexible condition definitions for race eligibility
- Configurable randomization and fairness parameters

### Sponsor Management
- Dynamic prize pool definitions
- Seasonal limit enforcement
- Sticker requirement mapping
- Multi-event tracking capabilities

This system represents a mature, production-ready solution for managing complex prize raffles in the motorsports industry, with emphasis on fairness, transparency, auditability, and scalability.
